#:package FileBasedApp.Toolkit@1.0.1
#:property PublishAot=false
using System.ComponentModel;
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FileBasedApp.Toolkit.CommandCli;
using NuGet.Versioning;

var commandApp = new CommandApp<RunCommand>().WithDescription("Enter the description here");
commandApp.Configure(ctx =>
{
    ctx.PropagateExceptions();
}); 
return await commandApp.RunAsync(args);
public partial class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
    [GeneratedRegex(@"^alpha\-(?<alphaversion>\d+)$")]
    private partial Regex Alpha { get; }

    [GeneratedRegex(@"^rc\-(?<rcversion>\d+)$")]

    private partial Regex Rc { get; }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var buildProps = settings.ExecutionPath / ".." / "Source" / "FileBasedApp" / "Directory.Build.props";

        if (!buildProps.FileExists())
        {
            throw new FileNotFoundException($"Directory {buildProps.Value} does not exist");
        }

        XElement xml;
        await using (var fileSystemStream = buildProps.OpenRead())
        {
            xml = await XElement.LoadAsync(fileSystemStream, LoadOptions.None, cancellationToken);
        }
            
        var versionPrefix = xml.Descendants("VersionPrefix").FirstOrDefault() ??
                            throw new InvalidOperationException("Failed to find VersionPrefix");

        if (!NuGetVersion.TryParse(versionPrefix.Value, out var version))
        {
            throw new InvalidOperationException($"The version {version} could not be parsed");
        }

        var versionSuffix = xml.Descendants("VersionSuffix").FirstOrDefault();

        void UpdateVersion(string newVersion)
        {
            AnsiConsole.MarkupLineInterpolated($"[dim]Updating version from {versionPrefix.Value} to {newVersion}[/]");
            versionPrefix.Value = newVersion;
        }

        void AddOrUpdateSuffix(string newSuffix)
        {
            AnsiConsole.MarkupLineInterpolated($"[dim]Adding or updating suffix from {newSuffix}[/]");
            if (versionSuffix is null)
            {
                versionSuffix = new XElement("VersionSuffix", newSuffix);
                versionPrefix.Parent!.Add(versionSuffix);
            }
            else
            {
                versionSuffix.Value = newSuffix;    
            }
        }
            
        switch (settings.BumpType)
        {
            case RunCommand.BumpType.Major:
                UpdateVersion(version.WithMajor(version.Major + 1).ToString());
                break;
            case RunCommand.BumpType.Minor:
                UpdateVersion(version.WithMinor(version.Minor + 1).ToString());
                break;
            case RunCommand.BumpType.Patch:
                UpdateVersion(version.WithPatch(version.Patch + 1).ToString());
                break;
            case RunCommand.BumpType.Alpha:
                if (versionSuffix?.Value == null)
                {
                    AddOrUpdateSuffix("alpha-01");
                }
                else if (Alpha.Match(versionSuffix.Value) is {Success: true} match)
                {
                    var newVersion = match.Groups["alphaversion"].Value.RequiredParse<int>() + 1;
                    AddOrUpdateSuffix($"alpha-{newVersion:D2}");
                }
                else
                {
                    throw new InvalidOperationException($"Version suffix does not match alpha layout {versionSuffix.Value}");
                }
                break;
            case RunCommand.BumpType.RC:
                if (versionSuffix?.Value == null || !Rc.IsMatch(versionSuffix.Value))
                {
                    AddOrUpdateSuffix("rc-01");
                }
                else if (Rc.Match(versionSuffix.Value) is {Success: true} match)
                {
                    var newVersion = match.Groups["rcversion"].Value.RequiredParse<int>() + 1;
                    AddOrUpdateSuffix($"rc-{newVersion:D2}");
                }
                else
                {
                    throw new InvalidOperationException($"Version suffix does not match rc layout {versionSuffix.Value}");
                }
                break;
            case RunCommand.BumpType.Release:
                AnsiConsole.MarkupLineInterpolated($"[dim]Removing suffix {versionSuffix?.Value} for release version[/]");
                versionSuffix?.Remove();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
            
        await using var fileSystemStreamWrite = buildProps.OpenWrite();
        await AnsiConsole.Status().StartAsync("Saving new version", async ctx =>
        {
            await xml.SaveAsync(fileSystemStreamWrite, SaveOptions.None, cancellationToken);
        });
        
        AnsiConsole.MarkupLineInterpolated($"[green]Version updated successfully in {buildProps.Value}[/]");

        return 0; // 0 for success
    }

    /// <summary>
    /// The bump type
    /// </summary>
    public enum BumpType
    {
        /// <summary>
        ///  Bump the major version
        /// </summary>
        Major,
        /// <summary>
        /// Bump the minor version
        /// </summary>
        Minor,
        /// <summary>
        /// Bump the patch version
        /// </summary>
        Patch,
        /// <summary>
        /// Bump the alpha version. If not present it will be added
        /// </summary>
        Alpha,
        /// <summary>
        /// Bump release candidate version. If not present it will be added
        /// </summary>
        RC,
        /// <summary>
        /// Bump the release version. If not present it will be added
        /// </summary>
        Release
        
    }
    
    public class Settings : ExtendedCommandSettings 
    {
        /// <summary></summary>
        public AbsolutePath ExecutionPath { get; private set; }
        
        [CommandArgument(0, "<BumpType>")]
        [Description("Allowed versions Major, Minor, Patch, Alpha, RC, Release")]
        public BumpType BumpType { get; set; }

        protected override ValidationResult DoValidate()
        {
            // Exceptions here will bubble up and outputted as validation		
            // This will evaluate the path. If the path is relative, it will relative (in this case) against the execution folder. That would be the
            // directory that this .cs lives in
            ExecutionPath = this.TryGetDirectory(null, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);
            return base.DoValidate();
        }
    }
}