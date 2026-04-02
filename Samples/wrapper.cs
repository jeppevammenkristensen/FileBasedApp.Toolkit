#:package FileBasedApp.Toolkit@0.17.1-alpha-02
#:package Microsoft.CodeAnalysis.CSharp@5.3.0
#:package TextCopy@6.2.1
#:package Roslynator.CSharp@4.15.0
#:package Dumpify@*
#:property PublishAot=false

using System.Text;
using Spectre.Console.Cli;
using Spectre.Console;
using FileBasedApp.Toolkit.CommandCli;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

using TextCopy;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


var commandApp = new CommandApp<RunCommand>();
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	/// <summary>
	/// Executes the command asynchronously to generate a wrapper class and interface for public methods from the input source code.
	/// </summary>
	/// <param name="context">The command context containing information about the command execution environment.</param>
	/// <param name="settings">The command settings containing configuration options such as class name, interface name, and the compilation unit to process.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation, containing an integer result where 0 indicates successful execution.</returns>
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
		CancellationToken cancellationToken)
	{
		// Note that in this iteration this only wraps methods
		
		var @class = await ExtractClass(settings);
		
		var className = settings.ClassName ?? @class.Identifier.ValueText + "Wrapper";
		var interfaceName = settings.InterfaceName ?? "I" + className + "Wrapper";
		
		var methods = ExtractMethods(@class, className);

		var classBuilder = new StringBuilder();
		var interfaceBuilder = new StringBuilder();
		
		classBuilder.AppendLine($"public class {className} : {interfaceName}")
			.AppendLine("{");
		
		var isStatic = methods.Any(x => x.modiferListInfo.IsStatic);

		interfaceBuilder.AppendLine($"public interface {interfaceName}").AppendLine("{");

		if (!isStatic)
		{
			classBuilder.AppendLine($"private readonly {@class.Identifier.ValueText} _wrapper = new();");
		}

		var wrapperWriter = new WrapperMethodRewriter(isStatic, @class.Identifier.ValueText);
		
		foreach (var (_, method) in methods)
		{
			var interfaceMethod = ModifierList.Remove(method, SyntaxKind.PublicKeyword).WithBody(null).WithExpressionBody(null);
			interfaceMethod = ModifierList.Remove(interfaceMethod, SyntaxKind.StaticKeyword);
			
			interfaceBuilder.AppendLine($"\t{interfaceMethod.NormalizeWhitespace().ToFullString()}");

			classBuilder.AppendLine(wrapperWriter.Visit(method).NormalizeWhitespace().ToFullString());
		}
		
		interfaceBuilder.AppendLine("}");
		classBuilder.AppendLine("}");

		var result = new StringBuilder();

		result.AppendLine(ParseCompilationUnit(interfaceBuilder.ToString()).NormalizeWhitespace().ToFullString());
		result.AppendLine(ParseCompilationUnit(classBuilder.ToString()).NormalizeWhitespace().ToFullString());
		
		AnsiConsole.WriteLine(result.ToString());
		
		var copy = await AnsiConsole.ConfirmAsync($"[green]above Copy to clipboard?[/]", false, cancellationToken);

		if (copy)
		{
			await ClipboardService.SetTextAsync(result.ToString(), cancellationToken);	
		}
		
		
		return 0; // 0 for success
	}

	private static List<(ModifierListInfo modiferListInfo, MethodDeclarationSyntax method)> ExtractMethods(ClassDeclarationSyntax @class, string className)
	{
		var methods = @class.Members
			.OfType<MethodDeclarationSyntax>()
			.Where(x =>
			{
				var modifierListInfo = SyntaxInfo.ModifierListInfo(x);
				return modifierListInfo.GetFilter().HasFlag(ModifierFilter.Public);
			})
			.Select(x => (modiferListInfo:SyntaxInfo.ModifierListInfo(x), method: x ))
			.ToList();

		if (methods.Count == 0)
		{
			throw new InvalidOperationException("No methods found");
		}

		if (!(methods.All(x => x.modiferListInfo.IsStatic) || methods.All(x => !x.modiferListInfo.IsStatic)))
		{
			throw new InvalidOperationException($"All methods must either be static or all must be non static for class {className}");
		}

		return methods;
	}

	private static async Task<ClassDeclarationSyntax> ExtractClass(Settings settings)
	{
		var classes = settings.CompilationUnitSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

		if (classes.Count == 0)
		{
			throw new InvalidOperationException("No classes found in the clipboard");
		}

		var result = classes[0];

		if (classes.Count > 1)
		{
			result = await AnsiConsole.PromptAsync(new SelectionPrompt<ClassDeclarationSyntax>()
				.Title("Select the class to generate")
				.AddChoices(classes)
				.UseConverter(c => c.Identifier.ValueText));
		}

		return result;
	}

	public class Settings : ExtendedCommandSettings
	{
		[CommandOption("--className")]
		public string? ClassName { get; set; }
		
		[CommandOption("--interfaceName")]
		public string? InterfaceName { get; set; }

		public CompilationUnitSyntax CompilationUnitSyntax { get; private set; } = null!;

		protected override ValidationResult DoValidate()
		{
			var clipboardValue = TextCopy.ClipboardService.GetText();
			if (string.IsNullOrWhiteSpace(clipboardValue))
			{
				return ValidationResult.Error("Clipboard is empty");
			}

			var compilationUnitSyntax = SyntaxFactory.ParseCompilationUnit(clipboardValue);
			if (compilationUnitSyntax.ContainsDiagnostics)
			{
				return ValidationResult.Error("The value from the clipboard contains errors");
			}
			
			CompilationUnitSyntax = compilationUnitSyntax;
			 
			return base.DoValidate();
		}
	}

	public class WrapperMethodRewriter(bool isStatic, string? className) : CSharpSyntaxRewriter
	{
		public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			node = ModifierList.Remove(node, SyntaxKind.StaticKeyword);
			
			var invocation = InvocationExpression(ParseExpression((isStatic ? className! : "_wrapper") + "." + node.Identifier.ValueText));

			var argumentSyntaxes = node.ParameterList.Parameters.Select(x => Argument(IdentifierName(x.Identifier.ValueText)));

			invocation = invocation.WithArgumentList(ArgumentList(SeparatedList(argumentSyntaxes)));

			return node
				.WithBody(null)
				.WithExpressionBody(ArrowExpressionClause(invocation));
		}
	}
}