#:package FileBasedApp.Toolkit@0.9.0.11
#:package FileBasedApp.Toolkit.SourceGenerators@*

using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SourceGenerators;
using Spectre.Console.Cli;
using Spectre.Console;
using TruePath;
using System.ComponentModel;
using System.Text.RegularExpressions;

CommandApp<TestsCommand> app = new();

app.Configure(cfg => {
	//cfg.PropagateExceptions();	
});
await app.RunAsync(args);

public class TestsCommand : AsyncCommand<TestSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, TestSettings settings, CancellationToken cancellationToken)
	{
		AnsiConsole.MarkupLineInterpolated($"[green]{settings.Path_Path.Value}[/]");
		return 0;
	}
}

public partial class TestSettings : ExtendedCommandSettings
{
	[CommandOption("--path <PATH>")]
	[DirectoryPath(true,false, PredefinedRootPath.CurrentDirectory)]
	public string Path { get; set; }
	
	[GeneratedRegex("a")]
	public partial Regex Method();
	
	protected override ValidationResult DoValidate()
	{
		
		_ = Path_Path;
		return base.DoValidate();
	}
	
}
