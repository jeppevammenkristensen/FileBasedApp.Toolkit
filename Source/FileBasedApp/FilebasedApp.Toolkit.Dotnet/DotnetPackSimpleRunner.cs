using FileBasedApp.Toolkit;
using TruePath;

namespace FilebasedApp.Toolkit.Dotnet;

/// <summary>
/// Fluent wrapper around <c>dotnet nuget push</c> — pushes a NuGet package to a server and publishes it.
/// </summary>
/// <remarks>May not be exhaustive. Remember you can use existing logic like AddArgument etc. to add</remarks>
public class DotnetNugetPushSimpleRunner : DotnetBaseRunner<DotnetNugetPushSimpleRunner>
{
    public static DotnetNugetPushSimpleRunner Init() => new DotnetNugetPushSimpleRunner().AddArguments("nuget","push");

    /// <summary>
    /// Sets the path to the package to push. Must be called before any other arguments.
    /// </summary>
    public DotnetNugetPushSimpleRunner WithPackage(AbsolutePath packagePath)
    {
        Arguments = Arguments.Insert(2, packagePath.Value);
        
        return this;
    }

    /// <summary>
    /// Sets the package source (URL, UNC/folder path or package source name) to use.
    /// Defaults to <c>DefaultPushSource</c> if specified in NuGet.Config (<c>--source</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithSource(string source) => AddArgumentPair("--source", source);

    /// <summary>
    /// Sets the API key for the server (<c>--api-key</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithApiKey(string apiKey, bool isSecret = true) => 
        AddArgumentPair("--api-key", apiKey, isSecret);
    
    public DotnetNugetPushSimpleRunner WithConditionalApiKey(string? apiKey, bool isSecret = true) => 
        AddArgumentPairIfValueNotEmpty("--api-key", apiKey, StringNullCheck.Null, isSecret);

    /// <summary>
    /// Sets the API key for the symbol server (<c>--symbol-api-key</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithSymbolServiceApiKey(string symbolServerApiKey, bool isSecret = true) => AddArgumentPair("--symbol-api-key", symbolServerApiKey, isSecret);

    /// <summary>
    /// If a package and version already exists, skip it and continue with the next package in the push, if any (<c>--skip-duplicate</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithSkipDuplicate() => AddArgument("--skip-duplicate");

    /// <summary>
    /// Disables buffering when pushing to an HTTP(S) server to decrease memory usage (<c>--disable-buffering</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithDisableBuffering() => AddArgument("--disable-buffering");

    /// <summary>
    /// Sets the timeout for pushing to a server in seconds. Defaults to 300 seconds (5 minutes) (<c>--timeout</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithTimeoutSeconds(int timeoutSeconds)
    {
        if (timeoutSeconds < 0)
            throw new ArgumentException("Timeout must be a non-negative integer", nameof(timeoutSeconds));

        return AddArgumentPair("--timeout", timeoutSeconds.ToString());
    }

    /// <summary>
    /// Allows the command to block and require manual action for operations like authentication (<c>--interactive</c>).
    /// </summary>
    public DotnetNugetPushSimpleRunner WithInteractive() => AddArgument("--interactive");
}


/// <summary>
/// Fluent wrapper around <c>dotnet pack</c> — the .NET NuGet package packer.
/// Creates a NuGet package from a project, solution, or file-based program.
/// </summary>
public class DotnetPackSimpleRunner : DotnetBaseRunner<DotnetPackSimpleRunner>
{
    public static DotnetPackSimpleRunner Init() => new DotnetPackSimpleRunner().AddArgument("pack");

    /// <summary>
    /// Sets the project, solution, or C# file to pack. Must be called before any other arguments.
    /// </summary>
    public DotnetPackSimpleRunner WithProject(AbsolutePath projectPath)
    {
        Arguments = Arguments.Insert(1, projectPath.Value);
        return this;
    }

    /// <summary>
    /// Sets the output directory to place built packages in (<c>--output</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithOutput(AbsolutePath outputPath, bool isSecret = false)
    {
        return AddArgumentPair("--output", outputPath, isSecret);
    }

    /// <summary>
    /// Sets the artifacts path. All output from the project, including build, publish, and pack output,
    /// will go in subfolders under the specified path (<c>--artifacts-path</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithArtifactsPath(AbsolutePath directory, bool isSecret = false)
    {
        return AddArgumentPair("--artifacts-path", directory, isSecret);

    }

    /// <summary>
    /// Includes packages with symbols in addition to regular packages in the output directory (<c>--include-symbols</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithIncludeSymbols() => AddArgument("--include-symbols");

    /// <summary>
    /// Includes PDBs and source files. Source files go into the <c>src</c> folder in the resulting NuGet package (<c>--include-source</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithIncludeSource() => AddArgument("--include-source");

    /// <summary>
    /// Sets the serviceable flag in the package (<c>--serviceable</c>).
    /// See https://aka.ms/nupkgservicing for more information.
    /// </summary>
    public DotnetPackSimpleRunner WithServiceable() => AddArgument("--serviceable");

    /// <summary>
    /// Suppresses the startup banner and copyright message (<c>--nologo</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithNoLogo() => AddArgument("--no-logo");

    /// <summary>
    /// Skips restoring the project before building (<c>--no-restore</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithNoRestore() => AddArgument("--no-restore");

    /// <summary>
    /// Allows the command to stop and wait for user input or action, e.g. to complete authentication (<c>--interactive</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithInteractive() => AddArgument("--interactive");

    /// <summary>
    /// Uses the current runtime as the target runtime (<c>--use-current-runtime</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithCurrentRuntime() => AddArgument("--use-current-runtime");

    /// <summary>
    /// Sets the target runtime to build for (<c>--runtime</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithRuntime(string runtime) => AddArgumentPair("--runtime", runtime);

    /// <summary>
    /// Sets the value of the <c>$(VersionSuffix)</c> property to use when building the project (<c>--version-suffix</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithVersionSuffix(string versionSuffix) => AddArgumentPair("--version-suffix", versionSuffix);

    /// <summary>
    /// Skips building project-to-project references and only packs the specified project (<c>--no-dependencies</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithNoDependencies() => AddArgument("--no-dependencies");

    /// <summary>
    /// Sets the version of the package to create (<c>--version</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithVersion(string version) => AddArgumentPair("--version", version);

    /// <summary>
    /// Skips building the project before packing. Implies <c>--no-restore</c> (<c>--no-build</c>).
    /// </summary>
    public DotnetPackSimpleRunner WithNoBuild()
    {
        return AddArgument("--no-build");
    }
}
