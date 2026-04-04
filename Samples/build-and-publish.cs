#:package FileBasedApp.Toolkit@0.17.1-rc-01
#:package FileBasedApp.Toolkit.CSharp@0.18.0-alpha-11
#:property PublishAot=false
#:property VersionPrefix=0.0.6
#:property PackageId=FileBasedApp.BuildAndPublish


using System.Collections.Immutable;
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.CSharp;
using TruePath.TestableIO.System.IO;

var commandApp = new CommandApp<RunCommand>();

commandApp.Configure(ctx =>
{
	ctx.PropagateExceptions();
});

return await commandApp.RunAsync(args);

public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	[GeneratedRegex(@"\d+\.\s+(?<name>.+?)\s\[(?<status>Enabled|Disabled)\]")]
	private partial Regex NugetSourceRegex { get; }
	
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		var temporaryDirectory  = (PathIO.GetTempPathAbsolute() / "Artifacts").CreateDirectory().GetAbsolutePath();
		AnsiConsole.MarkupLineInterpolated($"[green]Creating temporary {temporaryDirectory}[/]");
		
		try
		{		
			foreach (var path in settings.Path)
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Building [bold]{path.RelativeTo(PathUtil.GetCurrentWorkingFolder())}[/][/]");
				
				await SimpleExecRunner.Init("dotnet")
					.AddArgumentPair("pack", path)
					.AddArgumentPair("-c", "Release")
					.AddArgumentPair("-o", temporaryDirectory)
					.RunAsync(token: cancellationToken);
			}

			(string output, string _)  = await SimpleExecRunner.Init("dotnet")
				.AddArguments("nuget", "list", "source")
				.ReadAsync(token: cancellationToken);
			
			var reader = new StringReader(output);
			var line = await reader.ReadLineAsync(cancellationToken);
			var sources = new List<string>();

			while (line != null)
			{
				if (NugetSourceRegex.Match(line) is {Success: true} match && match.Groups["status"].Value == "Enabled")
				{
					sources.Add(match.Groups["name"].Value);
				}
				
				line = await reader.ReadLineAsync(cancellationToken);
			}
			
			string source = settings.Source;
			
			if (source.IsNullOrWhitespace())
			{
				source = await AnsiConsole.PromptAsync(new SelectionPrompt<string>().Title("Select source").AddChoices(sources), cancellationToken);
			}			
			
			
			foreach (var absolutePath in temporaryDirectory.GetFiles("*.nupkg").OrderBy(x => x.GetExtensionWithoutDot().StartsWith("s", StringComparison.OrdinalIgnoreCase) ? 1 : 0))
			{
				AnsiConsole.MarkupLineInterpolated($"[dim]{absolutePath.RelativeTo(temporaryDirectory)}[/]");
				
				var runner = SimpleExecRunner.Init("dotnet")
					.AddArgument("nuget")
					.AddArgumentPair("push", absolutePath)
					.AddArgumentPair("--source", source);
					
				if (!settings.ApiKey.IsNullOrWhitespace())
				{
					runner.AddArgumentPair("--api-key", settings.ApiKey!, true);
				}
				await runner.RunAsync(token: cancellationToken);
			}
		}
		finally
		{
			temporaryDirectory.DirectoryDelete(true);
		}
		
		return 0; // 0 for success
	}
	
	


	
	
	public class Settings : ExtendedCommandSettings
	{
		[CommandArgument(0, "[PathToUse]")]
		public string? PathCandidate { get; set; }
		
		[CommandOption("--api-key")]
		public string? ApiKey {get;set;}
		
		[CommandOption("--source")]
		public string? Source {get;set;}
		
		private readonly HashSet<string> _directlyBuildableTypes =
			new(["csproj", "slnx", "sln"], StringComparer.OrdinalIgnoreCase);
		
		public bool IsValidFile { get; private set; }
		public bool IsValidFolder { get; private set; }
		public ImmutableArray<AbsolutePath> Path { get; private set; }
		
		protected override ValidationResult DoValidate()
		{
			// Exceptions here will bubble up and outputted as validation
			var evaluatedPath = PathUtil.AnalyzeFile(PathCandidate, PathUtil.GetCurrentWorkingFolder());
			if (evaluatedPath.GetPath(shouldExist: true, false) is {errorMessage: null, path: { } filePath})
			{
				if (_directlyBuildableTypes.Contains(filePath.GetExtensionWithoutDot()))
				{
					IsValidFile = true;
					Path = [filePath];
					return base.DoValidate();					
				}
				else if (filePath.GetExtensionWithoutDot() == "cs")
				{
					var evaulator = new FileBasedAppEvaluator();
					if (evaulator.IsFileBasedApp(filePath))
					{
						IsValidFile = true;
						Path = [filePath];
						return base.DoValidate();
					}
					else
					{
						throw new InvalidOperationException($"The given cs file is not a file based app: {filePath}");
					}
				}

				throw new InvalidOperationException("The file is not in a correct format");
			}
			else
			{
				var evaulatedDirectory = PathUtil.AnalyzeDirectory(PathCandidate, PathUtil.GetCurrentWorkingFolder());
				if (evaulatedDirectory.GetPath(true, true) is {errorMessage: null, path: { } directoryPath})
				{
					var buildablePaths = directoryPath.EnumerateFiles().Where(path =>
					{
						if (_directlyBuildableTypes.Contains(path.GetExtensionWithoutDot()))
						{
							IsValidFile= true;
							return true;
						}

						if (path.GetExtensionWithoutDot() == "cs")
						{
							var evaulator = new FileBasedAppEvaluator();
							if (evaulator.IsFileBasedApp(path))
							{
								return true;
							}
						}

						return false;
					}).ToList();

					if (buildablePaths.Count == 0)
					{
						throw new InvalidOperationException($"The given directory is does not contain any buildable files: {directoryPath}");
					}


					Path = [..buildablePaths];
				}
			}
			
			if (Path == null)
			{
				throw new InvalidOperationException($"Path {PathCandidate} is not correct");
			}
    
			return base.DoValidate();
			
		}
	}
}