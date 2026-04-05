using FileBasedApp.Toolkit.SimpleExec;

namespace FileBasedApp.Toolkit.Dotnet;


/// <summary>
/// Base class for fluent <c>dotnet</c> CLI command runners. Provides common options shared across dotnet commands.
/// </summary>
public abstract class DotnetBaseRunner<TSelf> : BaseSimpleExecRunner<TSelf> where TSelf : DotnetBaseRunner<TSelf>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotnetBaseRunner{TSelf}"/> class with the command name set to "dotnet".
    /// </summary>
    protected DotnetBaseRunner() : base("dotnet")
    {
    }

    /// <summary>
    /// Sets the build configuration, e.g. <c>Debug</c> or <c>Release</c> (<c>--configuration</c>).
    /// </summary>
    public TSelf WithConfiguration(string configuration)
    {
        return AddArgumentPair("--configuration", configuration);
    }

    /// <summary>
    /// Sets the MSBuild verbosity level. Allowed values are <c>q[uiet]</c>, <c>m[inimal]</c>,
    /// <c>n[ormal]</c>, <c>d[etailed]</c>, and <c>diag[nostic]</c> (<c>--verbosity</c>).
    /// </summary>
    public TSelf WithVerbosity(string verbosity)
    {
        return AddArgumentPair("--verbosity", verbosity);
    }
}