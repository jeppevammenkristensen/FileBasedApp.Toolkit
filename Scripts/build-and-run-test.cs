#:package FileBasedApp.Toolkit@0.15.0-dev-01
#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using TruePath.TestableIO.System.IO;
using static SimpleExec.Command;

var commandApp = new CommandApp<RunCommand>();
await commandApp.RunAsync(args);

public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	[GeneratedRegex(".Tests+")]
	public static partial Regex TestsRegex();
	
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		AnsiConsole.MarkupLineInterpolated($"[green]Current working folder{PathUtil.GetCurrentWorkingFolder()}[/]");
		AnsiConsole.MarkupLineInterpolated($"[green]Current execution folder{PathUtil.GetExecutionFolder()}[/]");
		
		AnsiConsole.MarkupLineInterpolated($"[green]{settings.ParentFolder}[/]");
		var solutionFile = settings.ParentFolder.EnumerateFiles("FileBasedApp.Toolkit.slnx", SearchOption.AllDirectories).GetSingle(x => true);

		await RunAsync("dotnet", ["build", solutionFile.Value], ct: cancellationToken);

		var testReports = settings.ParentFolder / "test-reports";
		testReports.CreateDirectory();

		foreach (var absolutePath in settings.ParentFolder.EnumerateFiles("*.csproj", SearchOption.AllDirectories).Where(x => TestsRegex().IsMatch(x.FileName)))
		{
			AnsiConsole.MarkupLineInterpolated($"[green]Running tests for{absolutePath.RelativeTo(settings.ParentFolder)}[/]");
			List<string> testParameters = ["test", absolutePath.Value, $"--logger:trx;LogFileName={(testReports / (absolutePath.GetFilenameWithoutExtension() + ".trx")).Value}"];
			await RunAsync("dotnet", testParameters, settings.ParentFolder.Value, ct: cancellationToken);

		}

		

		return 0; // 0 for success
	}

	public class Settings : ExtendedCommandSettings
	{
		protected override ValidationResult DoValidate()
		{
			ParentFolder = PathUtil.GetExecutionFolder() / ".." ;
			
			
			// Exceptions here will bubble up and outputted as validation		

			// This will evaluate the path. If the path is relative, it will relative (in this case) against the execution folder. That would be the
			// directory that this .cs lives in
			_ = this.TryGetDirectory(ParentFolder.Value, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);         
    
			return base.DoValidate();
		}

		public AbsolutePath ParentFolder { get; set; }
	}
}