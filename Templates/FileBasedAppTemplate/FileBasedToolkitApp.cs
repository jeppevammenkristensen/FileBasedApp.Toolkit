#:package FileBasedApp.Toolkit@0.14.0
#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using TruePath.TestableIO.System.IO;

var commandApp = new CommandApp<RunCommand>();
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		AnsiConsole.MarkupLineInterpolated($"[green]SomePath is {settings.SomePathAbsolute.Value}[/]");

		var directoryInfo = DirectoryInfoFactory.New(settings.SomePathAbsolute);
		AnsiConsole.MarkupLineInterpolated($"[dim]Creation time: {directoryInfo.CreationTimeUtc}[/]");

		settings.SomePathAbsolute.DirectoryGetParent()!.GetAbsolutePath();

		var parentDirectory = settings.SomePathAbsolute / "..";
		
		
		AnsiConsole.Status().Start("Checking for .cs files", ctx =>
		{

			// Uses the extensions method to GetFiles
			foreach (var csfile in parentDirectory.EnumerateFiles("*.cs", SearchOption.AllDirectories).Take(50))
			{
				AnsiConsole.MarkupLineInterpolated($"[dim]Relative path {csfile.RelativeTo(settings.SomePathAbsolute)}[/]");
			}
		});

		await SimpleExec.Command.RunAsync("dotnet", ["--version"], parentDirectory.Value);
		return 0; // 0 for success
	}

	public class Settings : ExtendedCommandSettings
	{
		[CommandArgument(0, "[SomePath]")]
		public string? SomePath { get; set; }

		/// <summary></summary>
		public AbsolutePath SomePathAbsolute { get; private set; }
		protected override ValidationResult DoValidate()
		{
			// Exceptions here will bubble up and outputted as validation		

			// This will evaluate the path. If the path is relative, it will relative (in this case) against the execution folder. That would be the
			// directory that this .cs lives in
			SomePathAbsolute = this.TryGetDirectory(SomePath, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);         
    
			return base.DoValidate();
		}
	}
}