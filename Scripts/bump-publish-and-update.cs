#:package FileBasedApp.Toolkit@0.21.0
#:package AutoSpectre@0.12.0

using AutoSpectre;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using Spectre.Console;
using Test;
using TruePath;

var executionFolder = PathUtil.GetExecutionFolder();

var selector = new Selector();
selector.Prompt();

if (selector.BumpCondition)
{

    AnsiConsole.Write(new Rule($"Bump version {selector.BumpType}"));

    await SimpleExecRunner.Init("dotnet")
        .AddArgumentPair("run", executionFolder / "bump-version.cs")
        .AddArgument(selector.BumpType.ToString()).RunAsync();
}

if (selector.NugetSourceCondition)
{
    AnsiConsole.Write(new Rule("Publish to github package repository"));

    await SimpleExecRunner.Init("dotnet")
        .AddArgumentPair("run", executionFolder / ".." / "Samples" / "build-and-publish.cs")
        .AddArgument("--")
        .AddArgument(executionFolder / ".." / "Source")
        .AddArgumentPair("--source", "github")
        .RunAsync();
}

if (selector.UpdateVersion)
{
    await SimpleExecRunner.Init("dotnet")
        .AddArgumentPair("run", executionFolder / "update-filebased-package-references.cs")
        .AddArgumentPair("--exclude", PathUtil.GetExecutionFile().GetFilenameWithoutExtension())
        .RunAsync();
}


namespace Test
{
    [AutoSpectreForm]
    public class Selector
    {
        public string[] OperationSource = ["Bump", "Publish", "Update"];

        [SelectPrompt(Title = "Select the project(s) to bump version for")]
        public string[] Operation { get; set; } = [];

        public bool BumpCondition => Operation.Contains(OperationSource[0]);
        
        [TextPrompt(Title = "Select the version type to bump",
            Condition = nameof(BumpCondition))]
        public BumpType BumpType { get; set; }

        public string[] Sources = ["local", "github"];
        
        public bool NugetSourceCondition => Operation.Contains(OperationSource[1]);

        [SelectPrompt(Title = "Select nuget source", Source = nameof(Sources),
            Condition = nameof(NugetSourceCondition))]
        public string NugetSource { get; set; } = null!;
        
        public bool UpdateVersion => Operation.Contains(OperationSource[2]);
    }    
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
