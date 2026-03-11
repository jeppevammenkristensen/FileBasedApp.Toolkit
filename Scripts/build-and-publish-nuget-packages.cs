#:package FileBasedApp.Toolkit@*
#:property PublishAot=false

using FileBasedApp.Toolkit;
using Spectre.Console;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using TruePath;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Collections.Immutable;
using SimpleExec;

var commandApp = new CommandApp();

commandApp.Configure(ctx => {
	ctx.AddCommand<BuildTemplateCommand>("build-template").WithDescription("Build the template");	
	ctx.AddCommand<BuildCodeCommand>("build-code").WithDescription("Build the code");			
});

	
	
await commandApp.RunAsync(args);


public class BuildTemplateCommand : AsyncCommand<BuildTemplateCommand.Settings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		var fileSystem = new FileSystem();
		var codePath = PathUtil.GetExecutionFolder();

		var root = codePath / "..";
		var artifact = codePath / "TempTemplateArtifact";

		try
		{
			artifact.DirectoryDelete(true);
		}
		catch
		{
			// ignore
		}

		try
		{

			AbsolutePath projectFile = (root / "Templates" / "FileBasedAppTemplates.csproj");
			if (!fileSystem.File.Exists(projectFile)){
				throw new InvalidOperationException($"Project file {projectFile} was not found");
			}

			await SimpleExec.Command.RunAsync("dotnet", ["pack", projectFile.Value, "-c", "Release", "-o", artifact.Value]);
			var items = await Methods.GetNugetSources();

			// Prompt the use to select source
			var source = AnsiConsole.Prompt(new SelectionPrompt<string>()
				.Title("Select source")
				.AddChoices(items));
			
			// Enumerate all the generated nuget packages and publish them to the source
			foreach (var element in artifact.EnumerateFiles("*.*nupkg").OrderBy(x => x.GetExtensionWithoutDot().StartsWith("s") ? 1 : 0))
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Publishing {element.Value} to local source[/]");
				ImmutableArray<string> arguments = ["nuget", "push", element.Value, "--source", source,"--skip-duplicate"];

				var secrets = new List<string>();
				
				if (!string.IsNullOrWhiteSpace(settings.NugetApiKey))
				{
					secrets.AddRange("--api-key", settings.NugetApiKey);
				}
				
				try
				{
					await SimpleExec.Command.RunAsync("dotnet", arguments, secrets: secrets, ct: cancellationToken);
				}
				catch (ExitCodeReadException)
				{
					AnsiConsole.MarkupLineInterpolated($"[red]Could not publish {element.Value}[/]");
				}
			}

			AnsiConsole.MarkupLineInterpolated($"[green]Remove temp artifact [bold]{artifact}[/][/]");

		}

		finally

		{		
			artifact.DirectoryDelete(true);
		}

		return 0;
	}

	public class Settings : ExtendedCommandSettings
	{
		[CommandOption("--api-key")]
		public string? NugetApiKey { get; set; }	
	}
}


public class BuildCodeCommand : AsyncCommand<BuildCodeCommand.Settings>
{

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{		
		var fileSystem = new FileSystem();

		var codePath = PathUtil.GetExecutionFolder();

		var root = codePath / "..";
		var artifact = codePath / "TempArtifact";

		try
		{
			artifact.DirectoryDelete(true);	
		}
		catch {
			
		}
		
		

		try
		{				
			
			AbsolutePath solutionFile = root.GetFiles("*.slnx", SearchOption.AllDirectories).First();
	
			await SimpleExec.Command.RunAsync("dotnet", ["pack", solutionFile.Value, "-c", settings.Configuration, "-o", artifact.Value, "-p:IncludeSymbols=true", "-p:SymbolPackageFormat=snupkg"], ct: cancellationToken);	
			var items = await Methods.GetNugetSources();


			string? source = settings.NugetSource;
			
			if (settings.Interactive && string.IsNullOrWhiteSpace(source))
			{
				// Prompt the use to select source
				source = AnsiConsole.Prompt(new SelectionPrompt<string>()
					.Title("Select source")
					.AddChoices(items));	
			}
			
				
			if (settings.SkipDeploy)
			{
				return 0;
			}
	
			// Enumerate all the generated nuget packages and publish them to the source
			foreach (var element in artifact.EnumerateFiles("*.*nupkg").Where(x =>
			         {
				         if (settings.TruePathOnly)
				         {
					         return x.GetFilenameWithoutExtension().StartsWith("TruePath.");
				         }
				         else
				         {
					         return true;
				         }
			         }))
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Publishing {element.Value} to local source[/]");
				ImmutableArray<string> arguments = ["nuget", "push", element.Value, "--source", source, "--skip-duplicate"];
				
				if (!string.IsNullOrWhiteSpace(settings.NugetApiKey))
				{
					arguments = arguments.AddRange("--api-key", settings.NugetApiKey);
				}	
				
				
				try
				{	        
					await SimpleExec.Command.RunAsync("dotnet",arguments);
				}
				catch (ExitCodeReadException)
				{
					AnsiConsole.MarkupLineInterpolated($"[red]Could not publish {element.Value}[/]");					
				}
			}
	
			AnsiConsole.MarkupLineInterpolated($"[green]Remove temp artifact [bold]{artifact}[/][/]");

		}

		finally

		{
			if (!settings.SkipDeploy)
			{
				artifact.DirectoryDelete(true);				
			}
			

		}
		
		return 0;
	}

	public class Settings : FileBasedApp.Toolkit.ExtendedCommandSettings
	{
		[CommandOption("--api-key")]
		public string? NugetApiKey {get;set; }
		
		[CommandOption("--true-path-only")]
		public bool TruePathOnly { get; set; }
		
		protected override ValidationResult DoValidate()
		{
			if (string.IsNullOrWhiteSpace(NugetApiKey))
			{
				NugetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
			}
			
			if (!string.IsNullOrWhiteSpace(NugetApiKey))
			{
				Configuration = "Release";
			}

			if (!Interactive && string.IsNullOrWhiteSpace(NugetSource))
			{
				throw new InvalidOperationException("You must specify a source when not in interactive mode");
			}
			
			return base.DoValidate();
		}
		
		[CommandOption("-c|--configuration")]
		[DefaultValue("Debug")]
		public required string Configuration {get;set;}
		
		[CommandOption("--skip-deploy")]
		[DefaultValue("false")]
		public bool SkipDeploy {get;set;}
		
		[CommandOption("--interactive")]
		public bool Interactive { get; set; }
		
		[CommandOption("--source")]
		public string? NugetSource { get; set; }
		
		
		
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









