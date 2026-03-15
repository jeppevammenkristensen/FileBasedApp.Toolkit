#:package FileBasedApp.Toolkit@0.16.0-rc-01
#:package Microsoft.CodeAnalysis.CSharp@*
#:package TextCopy@*
#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using TruePath.TestableIO.System.IO;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

var commandApp = new CommandApp<RunCommand>()
	.WithDescription("Locates filebase apps and list them so their paths can be copied");
	
commandApp.Configure(ctx => {
	ctx.PropagateExceptions();
});

return await commandApp.RunAsync(args);

public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	[GeneratedRegex("^#:(package|property|sdk|project)")]
	protected partial Regex HasFileBasedDirective();
	
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		
		AnsiConsole.MarkupLineInterpolated($"[yellow]Path: [bold]{settings.Path}[/][/] ");		
		

		var paths = settings.Path.GetFiles("*.cs", SearchOption.AllDirectories).Where(EvaluateFile).ToList();
		
		var result = await AnsiConsole.PromptAsync(new SelectionPrompt<AbsolutePath>().Title("Select filebased app").AddChoices(paths).UseConverter(x => x.RelativeTo(settings.Path).Value));
		
		TextCopy.ClipboardService.SetText(result.Value);
		

		
		return 0; // 0 for success
	}

	private bool EvaluateFile(AbsolutePath path)
	{
		
		
		var compilationUnit = ParseCompilationUnit(path.ReadAllText());
		if (compilationUnit.Members.FirstOrDefault() is GlobalStatementSyntax)
		{
			return compilationUnit.GetLeadingTrivia().Any(x => HasFileBasedDirective().IsMatch(x.ToString()));
		}
		
		return false;
	}

	public class Settings : ExtendedCommandSettings
	{


		/// <summary></summary>
		public AbsolutePath SomePathAbsolute { get; private set; }
		
		[CommandArgument(0, "[Path]")]
		public string CustomPath { get; private set; }
		public AbsolutePath Path { get; private set; }

		protected override ValidationResult DoValidate()
		{
			// Exceptions here will bubble up and outputted as validation		

			Path = this.TryGetDirectory(CustomPath, true, true, rootPaths: [PathUtil.GetCurrentWorkingFolder(), PathUtil.GetExecutionFolder()]);
    
			return base.DoValidate();
		}
	}
}