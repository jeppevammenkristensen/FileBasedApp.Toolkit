#:package FileBasedApp.Toolkit@*

using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.Abstractions;
using Spectre.Console;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using TruePath;

var currentPath = PathUtil.GetCurrentWorkingFolder();
var fileSystem = new FileSystem();

var codePath = PathUtil.GetExecutionFolder();

var root = codePath / "..";
var artifact = codePath / "TempArtifact";

AbsolutePath solutionFile = fileSystem.Directory.GetFiles(root, "*.slnx", SearchOption.AllDirectories).First();

await SimpleExec.Command.RunAsync("dotnet", ["pack", solutionFile.Value, "-c", "Debug","-o", artifact.Value]);

var items = await Methods.GetNugetSources();

// Prompt the use to select source
var source = AnsiConsole.Prompt(new SelectionPrompt<string>()
	.Title("Select source")
	.AddChoices(items));

// Enumerate all the generated nuget packages and publish them to the source
foreach (var element in fileSystem.Directory.GetFiles(artifact, "*.nup*"))
{
	AnsiConsole.MarkupLineInterpolated($"[green]Publishing {element.Value} to local source[/]");
	await SimpleExec.Command.RunAsync("dotnet", ["nuget", "push", element.Value, "--source", source]);
}

AnsiConsole.MarkupLineInterpolated($"[green]Remove temp artifact [bold]{artifact}[/][/]");
fileSystem.Directory.Delete(artifact, true);


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









