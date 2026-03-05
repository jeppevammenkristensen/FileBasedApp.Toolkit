#:package FileBasedApp.Toolkit@*
#:property PackageAot=false

using FileBasedApp.Toolkit;
using Spectre.Console;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using TruePath;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Collections.Immutable;

var commandApp = new CommandApp<BuildCommand>().
	WithDescription("Helps build this solution for local or to nuget");
	
await commandApp.RunAsync(args);

public class BuildCommand : AsyncCommand<BuildCommand.Settings>
{

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		var currentPath = PathUtil.GetCurrentWorkingFolder();
		var fileSystem = new FileSystem();

		var codePath = PathUtil.GetExecutionFolder();

		var root = codePath / "..";
		var artifact = codePath / "TempArtifact";

		try
		{	
	
			AbsolutePath solutionFile = root.GetFiles("*.slnx", SearchOption.AllDirectories).First();
	
			await SimpleExec.Command.RunAsync("dotnet", ["pack", solutionFile.Value, "-c", settings.Configuration, "-o", artifact.Value]);
	
			var items = await Methods.GetNugetSources();
	
			// Prompt the use to select source
			var source = AnsiConsole.Prompt(new SelectionPrompt<string>()
				.Title("Select source")
				.AddChoices(items));
	
			// Enumerate all the generated nuget packages and publish them to the source
			foreach (var element in artifact.EnumerateFiles("*.nup*"))
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Publishing {element.Value} to local source[/]");
				ImmutableArray<string> arguments = ["nuget", "push", element.Value, "--source", source];
				
				if (!string.IsNullOrWhiteSpace(settings.NugetApiKey))
				{
					arguments = arguments.AddRange("--api-key", settings.NugetApiKey);
				}	
				
				
				await SimpleExec.Command.RunAsync("dotnet",arguments);
			}
	
			AnsiConsole.MarkupLineInterpolated($"[green]Remove temp artifact [bold]{artifact}[/][/]");

		}

		finally

		{

			fileSystem.Directory.Delete(artifact, true);	

		}
		
		return 0;
	}

	public class Settings : FileBasedApp.Toolkit.ExtendedCommandSettings
	{
		[
		CommandOption("--api-key")]
		public string? NugetApiKey {get;set; }
		
		protected override ValidationResult DoValidate()
		{			
			return base.DoValidate();
		}
		
		[CommandOption("-c|--configuration")]
		[DefaultValue("Debug")]
		public required string Configuration {get;set;}
	}
}





public class Methods 
{
	public static async Task<List<string>> GetNugetSources()
	{
		Regex regex = new(@"(?<test>\S+) \[Enabled\]");

		var result = await SimpleExec.Command.ReadAsync("dotnet", ["nuget", "list", "source"]);

		Console.WriteLine(result.StandardOutput);

		var stringReader = new StringReader(result.StandardOutput);
		var items = new List<string>();

		while (stringReader.Peek() >= 0)
		{
			var current = await stringReader.ReadLineAsync();
			if (current is { } && regex.Match(current) is { Success: true } match)
			{
				items.Add(match.Groups["test"].Value);
			}
		}

		return items;
	}
}









