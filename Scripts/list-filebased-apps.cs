#:package FileBasedApp.Toolkit@0.19.0-rc-02
#:package Microsoft.CodeAnalysis.CSharp@*
#:package TextCopy@*
#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using System.ComponentModel;
using FileBasedApp.Toolkit.CommandCli;

var commandApp = new CommandApp<RunCommand>()
	.WithDescription("Locates filebase apps and list them so their paths can be copied. It uses Roslyn in combination with regex to find file based directives like for instance :package to determine if the files are files based apps");
	
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
		
		if (settings.NonInteractive)
		{
			foreach (var element in paths)
			{
				AnsiConsole.MarkupLineInterpolated($"[green]{element.RelativeTo(settings.Path)}[/]");
			}
			return 0;
		}
			
		
		var result = await AnsiConsole.PromptAsync(new SelectionPrompt<AbsolutePath>().Title("Select filebased app").AddChoices(paths).UseConverter(x => x.RelativeTo(settings.Path).Value));
		
		TextCopy.ClipboardService.SetText(result.Value);
		

		
		return 0; // 0 for success
	}

	private bool EvaluateFile(AbsolutePath path)
	{
		
		
		var compilationUnit = ParseCompilationUnit(path.ReadAllText());
		// Note there is a risk that the GlobalStatementSyntax check can result in some 
		// filebasedapps failing this check. 
		if (compilationUnit.Members.FirstOrDefault() is GlobalStatementSyntax)
		{
			return compilationUnit.GetLeadingTrivia().Any(x => HasFileBasedDirective().IsMatch(x.ToString()));
		}
		
		return false;
	}

	public class Settings : ExtendedCommandSettings
	{		
		
		[CommandArgument(0, "[Path]")]
		[Description("The folder to traverse for file based apps")]
		public string? CustomPath { get; private set; }
		
		[CommandOption("--non-interactive")]
		[Description("When this is toggled no select prompt will occur")]
		public bool NonInteractive {get;set;}
		
		
		public AbsolutePath Path { get; private set; }

		protected override ValidationResult DoValidate()
		{
			// Exceptions here will bubble up and outputted as validation		

			Path = this.TryGetDirectory(CustomPath, true, true, rootPaths: [PathUtil.GetCurrentWorkingFolder(), PathUtil.GetExecutionFolder()]);
    
			return base.DoValidate();
		}
	}
}