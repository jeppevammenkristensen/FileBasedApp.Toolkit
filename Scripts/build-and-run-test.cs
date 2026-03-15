#:package FileBasedApp.Toolkit@0.16.0-rc-01
#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.SimpleExec;
using TruePath.TestableIO.System.IO;
using static SimpleExec.Command;

var commandApp = new CommandApp<RunCommand>();
return await commandApp.RunAsync(args);

public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	[GeneratedRegex(".Tests+")]
	public static partial Regex TestsRegex();
	
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		AnsiConsole.MarkupLineInterpolated($"[green]Current working folder{PathUtil.GetCurrentWorkingFolder()}[/]");
		AnsiConsole.MarkupLineInterpolated($"[green]Current execution folder{PathUtil.GetExecutionFolder()}[/]");
		
		AnsiConsole.MarkupLineInterpolated($"[green]{settings.ParentFolder}[/]");
		var solutionFile = settings.ParentFolder.EnumerateFiles("FileBasedApp.Toolkit.slnx", SearchOption.AllDirectories).GetSingleRequired(x => true, "Failed to locate FileBasedApp.Toolkit.slnx");

		foreach (var fileBased in DirectoryIO.GetFiles(settings.ParentFolder / "Scripts", "*.cs")
			         .Concat([
				         settings.ParentFolder.GetFiles("FileBasedToolkitApp.cs", SearchOption.AllDirectories)
					         .GetSingleRequired(x => x.Parent!.Value.FileName.Contains("Template",
						         StringComparison.OrdinalIgnoreCase), "Failed to locate FileBasedToolkitApp.cs")
			         ]))
		{
			if (fileBased.Equals(PathUtil.GetExecutionFile()))
				continue;

			AnsiConsole.MarkupLineInterpolated($"[green]Building filebased app {fileBased.FileName}[/]");
			await new SimpleExecRunner("dotnet").AddArgumentPair("build", fileBased).RunAsync(token: cancellationToken);
		}
		
		await new SimpleExecRunner("dotnet").AddArgumentPair("build", solutionFile)
			.RunAsync(token: cancellationToken);

		var testReports = settings.ParentFolder / "test-reports";
		testReports.CreateDirectory();

		foreach (var absolutePath in settings.ParentFolder.EnumerateFiles("*.csproj", SearchOption.AllDirectories).Where(x => TestsRegex().IsMatch(x.FileName)))
		{
			AnsiConsole.MarkupLineInterpolated($"[green]Running tests for [bold]{absolutePath.RelativeTo(settings.ParentFolder)}[/][/]");
			List<string> testParameters = ["test", absolutePath.Value, $"--logger:trx;LogFileName={(testReports / (absolutePath.GetFilenameWithoutExtension() + ".trx")).Value}"];
			
			await new SimpleExecRunner("dotnet")
				.AddArgumentPair("test", absolutePath)
				.AddArgument($"--logger:trx;LogFileName={(testReports / (absolutePath.GetFilenameWithoutExtension() + ".trx")).Value}").RunAsync(token: cancellationToken);
			
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