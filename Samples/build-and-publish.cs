#:package FileBasedApp.Toolkit@0.20.0
#:package FileBasedApp.Toolkit.CSharp@0.20.0
#:package FileBasedApp.Toolkit.Dotnet@0.20.0
#:property PublishAot=false
#:property VersionPrefix=0.0.9
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
using FileBasedApp.Toolkit.Dotnet;
using System.ComponentModel;

// You can use this app to install itself
// Run dotnet run build-and-publish.cs -- build-and-publish.cs 
// and select a relevant nuget source. Most likely a local source is preferable
// then afterwards you can install/update it with dotnet tool install FileBasedApp.BuildAndPublish -g
// remember to bump the VersionPrefix 
// then you can call build-and-publish.exe from everywhere

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
	
	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		// Stage all produced .nupkg files in a temp folder so they can be pushed in a single sweep below
		var temporaryDirectory  = (PathIO.GetTempPathAbsolute() / "Artifacts").CreateDirectory().GetAbsolutePath();
		AnsiConsole.MarkupLineInterpolated($"[green]Creating temporary {temporaryDirectory}[/]");

		try
		{
			// Pack each input target (csproj/sln/slnx/file-based .cs) into the shared temp output directory
			foreach (var path in settings.Path)
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Building [bold]{path.RelativeTo(PathUtil.GetCurrentWorkingFolder())}[/][/]");

				var packRunner = DotnetPackSimpleRunner
					.Init()
					.WithProject(path)
					.WithConfiguration("Release")
					.AddArguments(arguments: ["-p:IncludeSymbols=true", "-p:SymbolPackageFormat=snupkg"])
					.WithOutput(temporaryDirectory)
					.RunAsync(token: cancellationToken);
			}

			// Discover the user's configured nuget sources by parsing `dotnet nuget list source` output
			(string output, string _)  = await SimpleExecRunner.Init("dotnet")
				.AddArguments("nuget", "list", "source")
				.ReadAsync(token: cancellationToken);

			var reader = new StringReader(output);
			var line = await reader.ReadLineAsync(cancellationToken);
			var sources = new List<string>();

			// Keep only sources marked Enabled — disabled ones cannot be pushed to
			while (line != null)
			{
				if (NugetSourceRegex.Match(line) is {Success: true} match && match.Groups["status"].Value == "Enabled")
				{
					sources.Add(match.Groups["name"].Value);
				}

				line = await reader.ReadLineAsync(cancellationToken);
			}

			string? source = settings.Source;

			// Fall back to an interactive picker when --source wasn't provided
			if (source.IsNullOrWhitespace())
			{
				source = await AnsiConsole.PromptAsync(new SelectionPrompt<string>().Title("Select source").AddChoices(sources), cancellationToken);
			}


			// Push every produced package; symbol packages (.snupkg) are pushed last because
			// some feeds reject them if the matching .nupkg isn't already present
			foreach (var absolutePath in temporaryDirectory.GetFiles("*.*nupkg")
				.OrderBy(x => x.GetExtensionWithoutDot().StartsWith("s", StringComparison.OrdinalIgnoreCase) ? 1 : 0))
			{
				AnsiConsole.MarkupLineInterpolated($"[dim]{absolutePath.RelativeTo(temporaryDirectory)}[/]");
				
				// SkipDuplicate avoids failures when republishing the same version to a feed that already has it
				var runner = DotnetNugetPushSimpleRunner
					.Init()
					.WithPackage(absolutePath)
					.WithSkipDuplicate()
					.WithSource(source);

				// API key is only required for remote feeds (e.g. nuget.org); local feeds work without one
				if (!settings.ApiKey.IsNullOrWhitespace())
				{
					runner.WithApiKey(settings.ApiKey);
				}

				await runner.RunAsync(token: cancellationToken);
			}
		}
		finally
		{
			// Always clean up the staging directory, even if a pack/push step failed
			temporaryDirectory.DirectoryDelete(true);
		}
		
		return 0; // 0 for success
	}	
	
	public partial class Settings : ExtendedCommandSettings
	{
		[CommandArgument(0, "[PathToUse]")]
		[Description("Path to a .csproj, .sln, .slnx, a file-based .cs app, or a directory containing any of these. Used as the build target.")]
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
			// PathCandidate may point at either a single file or a directory — try the file interpretation first
			var evaluatedPath = PathUtil.AnalyzeFile(PathCandidate, PathUtil.GetCurrentWorkingFolder());
			if (evaluatedPath.GetPath(shouldExist: true, false) is {errorMessage: null, path: { } filePath})
			{
				// csproj/sln/slnx are buildable as-is by `dotnet pack`
				if (_directlyBuildableTypes.Contains(filePath.GetExtensionWithoutDot()))
				{
					IsValidFile = true;
					Path = [filePath];
					return base.DoValidate();
				}
				// A loose .cs file is only accepted if it is a file-based app (has top-level #:package directives)
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
				// Not a file — treat the candidate as a directory and collect every buildable thing in it
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