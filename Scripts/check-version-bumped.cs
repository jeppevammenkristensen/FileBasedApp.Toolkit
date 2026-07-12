#:package FileBasedApp.Toolkit@0.21.0-alpha-03
#:property PublishAot=false
using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.CommandCli;
using NuGet.Versioning;
using System.IO.Abstractions;
using System.Xml.Linq;
using FileBasedApp.Toolkit.SimpleExec;
using TruePath;

var commandApp = new CommandApp<RunCommand>().WithDescription(
    "Checks whether the FileBasedApp package version was increased compared to a previous commit. " +
    "Writes 'bumped=true|false' to $GITHUB_OUTPUT and warns (::warning::) when it was not increased.");
commandApp.Configure(ctx =>
{
    ctx.PropagateExceptions();
});
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    private const string RelativeBuildPropsPath = "Source/FileBasedApp/Directory.Build.props";

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var buildProps = settings.ExecutionPath / ".." / RelativeBuildPropsPath;

        var (currentVersion, suffixIsEmpty) = await ReadVersionAsync(buildProps, cancellationToken);
        if (suffixIsEmpty && !settings.PublishNoSuffix)
        {
            return await Finish(bumped: false, warning: "SuffixIsEmpty and PublishNoSuffix is set to false");
        }
        
        AnsiConsole.MarkupLineInterpolated($"[green]Current version: {currentVersion}[/]");

        if (string.IsNullOrWhiteSpace(settings.BeforeSha) || settings.BeforeSha.All(c => c == '0'))
        {
            return await Finish(bumped: false, warning: "No previous commit to compare against (first push to branch) - skipping auto-publish");
        }

        NuGetVersion previousVersion;
        try
        {
            var (output, _) = await SimpleExecRunner.Init("git")
                .AddArgumentPair("show", $"{settings.BeforeSha}:{RelativeBuildPropsPath}")
                .WithWorkingDirectory(settings.ExecutionPath).ReadAsync(token: cancellationToken);
            previousVersion = ParseVersion(XElement.Parse(output), out _);
            
        }
        catch (Exception ex)
        {
            return await Finish(bumped: false, warning: $"Could not read the previous version ({ex.Message}) - skipping auto-publish");
        }
        
        AnsiConsole.MarkupLineInterpolated($"[green]Previous version: {previousVersion}[/]");

        if (currentVersion.CompareTo(previousVersion) <= 0)
        {
            return await Finish(bumped: false, warning: $"Version was not increased ({previousVersion} -> {currentVersion}) - skipping auto-publish");
        }

        return await Finish(bumped: true);
    }

    private static async Task<int> Finish(bool bumped, string? warning = null)
    {
        if (warning is not null)
        {
            AnsiConsole.WriteLine($"::warning::{warning}");
        }

        var githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (!string.IsNullOrWhiteSpace(githubOutput))
        {
            await File.AppendAllTextAsync(githubOutput, $"bumped={(bumped ? "true" : "false")}{Environment.NewLine}");
        }

        return 0;
    }

    private static async Task<(NuGetVersion, bool)> ReadVersionAsync(AbsolutePath path, CancellationToken cancellationToken)
    {
        await using var stream = path.OpenRead();
        var xml = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var result = ParseVersion(xml, out var suffixIsEmpty);
        return (result, suffixIsEmpty);
    }

    private static NuGetVersion ParseVersion(XElement xml, out bool suffixIsEmpty)
    {
        suffixIsEmpty = false;
        
        var prefix = xml.Descendants("VersionPrefix").FirstOrDefault()?.Value
                     ?? throw new InvalidOperationException("Failed to find VersionPrefix");
        var suffix = xml.Descendants("VersionSuffix").FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(suffix))
        {
            suffixIsEmpty = true;
        }

        var versionString = string.IsNullOrWhiteSpace(suffix) ? prefix : $"{prefix}-{suffix}";
        if (!NuGetVersion.TryParse(versionString, out var version))
        {
            throw new InvalidOperationException($"Could not parse version '{versionString}'");
        }

        return version;
    }

    public class Settings : ExtendedCommandSettings
    {
        /// <summary></summary>
        public AbsolutePath ExecutionPath { get; private set; }

        [CommandArgument(0, "<BeforeSha>")]
        public string BeforeSha { get; set; } = "";

        [CommandOption("--publish-no-suffix")]
        [Description("If set will output that this should be published")]
        public bool PublishNoSuffix { get; set; }

        protected override ValidationResult DoValidate()
        {
            ExecutionPath = this.TryGetDirectory(null, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);
            return base.DoValidate();
        }
    }
}
