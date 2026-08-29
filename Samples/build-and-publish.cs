#:package FileBasedApp.Toolkit@1.0.1
#:package FileBasedApp.Toolkit.CSharp@1.0.1
#:package FileBasedApp.Toolkit.Dotnet@1.0.1
#:property PublishAot=false
#:property VersionPrefix=0.0.8
#:property PackageId=FileBasedApp.BuildAndPublish

using System.Collections.Immutable;
using System.ComponentModel;
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
	ctx.UseAssemblyInformationalVersion();
});

return await commandApp.RunAsync(args);

/// <summary>
/// Packs buildable inputs and publishes the resulting NuGet packages to a selected source.
/// </summary>
public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	[GeneratedRegex(@"\d+\.\s+(?<name>.+?)\s\[(?<status>Enabled|Disabled)\]")]
	private partial Regex NugetSourceRegex { get; }
	
	/// <summary>
	/// Packs the configured inputs and pushes the generated packages to the configured or interactively selected NuGet source.
	/// </summary>
	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		var temporaryDirectory  = (PathIO.GetTempPathAbsolute() / "Artifacts").CreateDirectory().GetAbsolutePath();
		AnsiConsole.MarkupLineInterpolated($"[green]Creating temporary {temporaryDirectory}[/]");
		
		temporaryDirectory.SafeDeleteDirectory();
		
		try
		{		
			foreach (var path in settings.Path)
			{
				AnsiConsole.MarkupLineInterpolated($"[green]Building [bold]{path.RelativeTo(PathUtil.GetCurrentWorkingFolder())}[/][/]");
				
				var runner = DotnetPackSimpleRunner
					.Init()
					.WithProject(path)
					.WithConfiguration("Release")
					.WithOutput(temporaryDirectory);

				if (settings.Path.Length == 1)
				{
					foreach (var (key, value) in settings.Properties ?? ImmutableDictionary<string, string?>.Empty)
					{
						runner.AddArgument($"-p:{key}={value}");
					}	
				}
				
				await runner.RunAsync(token: cancellationToken);					
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
			
			string? source = settings.Source;
			
			if (source.IsNullOrWhitespace())
			{
				source = await AnsiConsole.PromptAsync(new SelectionPrompt<string>().Title("Select source").AddChoices(sources), cancellationToken);
			}			
			
			
			foreach (var absolutePath in temporaryDirectory.GetFiles("*.nupkg").OrderBy(x => x.GetExtensionWithoutDot().StartsWith("s", StringComparison.OrdinalIgnoreCase) ? 1 : 0))
			{
				AnsiConsole.MarkupLineInterpolated($"[dim]{absolutePath.RelativeTo(temporaryDirectory)}[/]");
				
				var runner = DotnetNugetPushSimpleRunner
					.Init()
					.WithPackage(absolutePath)
					.WithSkipDuplicate()
					.WithSource(source);

				if (!settings.ApiKey.IsNullOrWhitespace())
				{
					runner.WithApiKey(settings.ApiKey);
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
	
	/// <summary>
	/// Defines the command-line inputs and validated build paths used by the build-and-publish workflow.
	/// </summary>
	public class Settings : ExtendedCommandSettings
	{
		/// <summary>
		/// Gets or sets the candidate project, solution, file-based app, or directory path to process.
		/// </summary>
		[CommandArgument(0, "[PathToUse]")]
		[Description("Path to a project, solution, file-based app, or directory containing buildable files.")]
		public string? PathCandidate { get; set; }
		
		/// <summary>
		/// Gets or sets the API key used to authenticate package pushes.
		/// </summary>
		[CommandOption("--api-key")]
		[Description("API key used when pushing packages to the selected NuGet source.")]
		public string? ApiKey {get;set;}
		
		/// <summary>
		/// Gets or sets the NuGet source name or URL to which packages are pushed.
		/// </summary>
		[CommandOption("--source")]
		[Description("NuGet source name or URL to push packages to. Prompts for a source when omitted.")]
		public string? Source {get;set;}
		
		/// <summary>
		/// Gets or sets the MSBuild properties supplied when packing each input.
		/// </summary>
		[CommandOption("-p|--property <NAME=VALUE>")]
		[Description("MSBuild property to pass to dotnet pack. May be specified multiple times.")]
		public IDictionary<string,string?> Properties { get; set; }
		
		private readonly HashSet<string> _directlyBuildableTypes =
			new(["csproj", "slnx", "sln"], StringComparer.OrdinalIgnoreCase);
		
		/// <summary>
		/// Gets a value indicating whether the supplied candidate resolves to a directly buildable file.
		/// </summary>
		public bool IsValidFile { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the supplied candidate resolves to a directory.
		/// </summary>
		public bool IsValidFolder { get; private set; }

		/// <summary>
		/// Gets the validated paths that will be packed and published.
		/// </summary>
		public ImmutableArray<AbsolutePath> Path { get; private set; }
		
		/// <summary>
		/// Resolves the path candidate and validates that it identifies at least one buildable input.
		/// </summary>
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

				if (filePath.GetExtensionWithoutDot() == "cs")
				{
					var evaulator = new FileBasedAppEvaluator();
					if (evaulator.IsFileBasedApp(filePath))
					{
						IsValidFile = true;
						Path = [filePath];
						return base.DoValidate();
					}

					throw new InvalidOperationException($"The given cs file is not a file based app: {filePath}");
				}

				throw new InvalidOperationException("The file is not in a correct format");
			}

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

			if (Path == null)
			{
				throw new InvalidOperationException($"Path {PathCandidate} is not correct");
			}
    
			return base.DoValidate();
			
		}
	}
}
