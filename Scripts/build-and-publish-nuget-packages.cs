#:package FileBasedApp.Toolkit@1.0.0
#:property PublishAot=false

using FileBasedApp.Toolkit;
using Spectre.Console;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using TruePath;
using Spectre.Console.Cli;
using System.ComponentModel;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.SimpleExec;
using SimpleExec;

var commandApp = new CommandApp();

commandApp.Configure(ctx =>
{
    ctx.PropagateExceptions();
    ctx.AddCommand<BuildTemplateCommand>("build-template").WithDescription("Build the template");
    ctx.AddCommand<BuildCodeCommand>("build-code").WithDescription("Build the code");
});

return await commandApp.RunAsync(args);


public abstract class BuildCommand<TSetting> : AsyncCommand<TSetting> where TSetting : ExtendedCommandSettings
{
    protected (AbsolutePath sourcePath, AbsolutePath artifactFolder) GetSourcePathAndOutput()
    {
        var codePath = PathUtil.GetExecutionFolder();
        var sourcePath = codePath / ".." / "Source";
        var artifactFolder = codePath / "Artifacts";

        AnsiConsole.MarkupLineInterpolated($"[green]Source path: [bold]{sourcePath}[/][/]");
        AnsiConsole.MarkupLineInterpolated($"[green]Artifact path: [bold]{artifactFolder}[/][/]");
        return (sourcePath, artifactFolder);
    }
}

public class BuildTemplateCommand : BuildCommand<BuildTemplateCommand.Settings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        var fileSystem = new FileSystem();

        var (sourcePath, artifact) = GetSourcePathAndOutput();

        artifact.SafeDeleteDirectory();
        try
        {
            AbsolutePath projectFile = sourcePath.GetFiles("FileBasedAppTemplates.csproj", SearchOption.AllDirectories)
                .GetSingleRequired(noMatchErrorMessage: "FileBasedAppTemplates.csproj was not found",
                    multipleMatchesError: "Multiple matches for FileBasedAppTemplates.csproj");

            if (!fileSystem.File.Exists(projectFile))
            {
                throw new InvalidOperationException($"Project file {projectFile} was not found");
            }

            await new SimpleExecRunner("dotnet")
                .AddArgument("pack")
                .AddArgument(projectFile)
                .AddArgumentPair("-c", "Release")
                .AddArgumentPair("-o", artifact).RunAsync(token: cancellationToken);

            var items = await Methods.GetNugetSources();

            string? source = settings.NugetSource;

            if (settings.Interactive && string.IsNullOrWhiteSpace(source))
            {
                // Prompt the use to select source
                source = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Select source")
                    .AddChoices(items));
            }

            // Enumerate all the generated nuget packages and publish them to the source
            foreach (var element in artifact.EnumerateFiles("*.*nupkg").OrderBy(x => x.GetExtensionWithoutDot()
                         .StartsWith("s")
                         ? 1
                         : 0))
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[green]Publishing {element.Value} to source [bold]{source}[/] [/]");

                var simpleExecRunner = new SimpleExecRunner("dotnet")
                    .AddArguments(isSecret: false, "nuget", "push")
                    .AddArgument(element)
                    .AddArgumentPair("--source", source!)
                    .AddArgument("--skip-duplicate");

                if (!string.IsNullOrWhiteSpace(settings.NugetApiKey))
                {
                    simpleExecRunner.AddArgument("--api-key").AddArgument(settings.NugetApiKey, true);
                }

                try
                {
                    await simpleExecRunner.RunAsync(token: cancellationToken);
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
            artifact.SafeDeleteDirectory();
        }

        return 0;
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandOption("--api-key")] public string? NugetApiKey { get; set; }

        [CommandOption("--interactive")] public bool Interactive { get; set; }

        [CommandOption("--source")] public string? NugetSource { get; internal set; }

        protected override ValidationResult DoValidate()
        {
            NugetApiKey = GetValueOrFromEnvironment(NugetApiKey, "NUGET_API_KEY");
            return base.DoValidate();
        }
    }
}


public class BuildCodeCommand : BuildCommand<BuildCodeCommand.Settings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        var (sourcePath, artifact) = GetSourcePathAndOutput();

        try
        {
            artifact.DirectoryDelete(true);
        }
        catch
        {
            // ignore
        }

        artifact.CreateDirectory();

        try
        {
            AbsolutePath solutionFile = sourcePath.GetFiles("*.slnx", SearchOption.AllDirectories)
                .GetSingleRequired(_ => true, noMatchErrorMessage: "Could not find a solution file",
                    multipleMatchesError: "Found more than one solution file");

            await new SimpleExecRunner("dotnet")
                .AddArgument("pack").AddArgument(solutionFile)
                .AddArgumentPair("-c", settings.Configuration)
                .AddArgumentPair("-o", artifact)
                .AddArguments(arguments: ["-p:IncludeSymbols=true", "-p:SymbolPackageFormat=snupkg"])
                .RunAsync(token: cancellationToken);

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
                         return settings.CodeToBuild switch
                         {
                             Settings.CodeToBuildType.FileBasedApp => x.GetFilenameWithoutExtension()
                                 .StartsWith("FileBasedApp."),
                             _ => true
                         };
                     }).OrderBy(x => x.GetExtensionWithoutDot().StartsWith('s') ? 1 : 0))
            {
                AnsiConsole.MarkupLineInterpolated($"[green]Publishing {element.Value} to local source[/]");
                var pushRunner = new SimpleExecRunner("dotnet")
                    .AddArguments(arguments: ["nuget", "push"])
                    .AddArgument(element)
                    .AddArgumentPair("--source", source!)
                    .AddArgument("--skip-duplicate");

                if (!string.IsNullOrWhiteSpace(settings.NugetApiKey))
                {
                    pushRunner.AddArgumentPair("--api-key", settings.NugetApiKey, isSecret: true);
                }

                try
                {
                    await pushRunner.RunAsync(token: cancellationToken);
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
                artifact.SafeDeleteDirectory();
            }
        }

        return 0;
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandOption("--api-key")] public string? NugetApiKey { get; set; }

        [CommandOption("--code-to-build", false)]
        [DefaultValue(CodeToBuildType.All)]
        [Description(
            $"The type to build. Values {nameof(CodeToBuildType.All)},{nameof(CodeToBuildType.FileBasedApp)}")]
        public CodeToBuildType CodeToBuild { get; set; }

        protected override ValidationResult DoValidate()
        {
            NugetApiKey = GetValueOrFromEnvironment(NugetApiKey, "NUGET_API_KEY");
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
        public required string Configuration { get; set; }

        [CommandOption("--skip-deploy")]
        [DefaultValue("false")]
        public bool SkipDeploy { get; set; }

        [CommandOption("--interactive")] public bool Interactive { get; set; }

        [CommandOption("--source")] public string? NugetSource { get; set; }


        // For now simple non flags enum
        public enum CodeToBuildType
        {
            [Description("Only FileBasedApp.Toolkit")]
            FileBasedApp,
            [Description("All packages")] All
        }
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
            if (current is { } && regex.Match(current) is {Success: true} match)
            {
                items.Add(match.Groups["test"].Value);
            }
        }

        return items;
    }
}
