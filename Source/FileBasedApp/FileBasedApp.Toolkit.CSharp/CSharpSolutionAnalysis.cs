using Microsoft.CodeAnalysis;
using Spectre.Console;
using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Provides functionality for analyzing C# solutions by loading and managing MSBuild workspaces.
/// This class handles the initialization of MSBuild environments, loading of C# projects, and
/// provides access to the underlying workspace and compilation information. Implements both
/// synchronous and asynchronous disposal patterns to properly clean up resources.
/// </summary>
/// <remarks>
/// This class serves as a wrapper around MSBuild workspace operations, providing a simplified
/// interface for loading and analyzing C# projects. It uses the Roslyn API for code analysis
/// and requires MSBuild to be properly configured on the system. The class maintains state
/// about whether a project has been loaded and ensures that initialization only occurs once
/// per instance. Thread safety considerations should be taken into account when using this
/// class in concurrent scenarios.
/// </remarks>
public sealed class CSharpSolutionAnalysis : BaseAnalysis<CSharpSolutionAnalysis>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CSharpSolutionAnalysis"/> using the default
    /// <see cref="Spectre.Console.AnsiConsole.Console"/> and a new <see cref="FileSystem"/> instance.
    /// </summary>
    public CSharpSolutionAnalysis()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CSharpSolutionAnalysis"/> using a custom <see cref="IAnsiConsole"/>
    /// and, optionally, a custom <see cref="IFileSystem"/>.
    /// </summary>
    /// <param name="console">
    /// The console to write status and diagnostic output to. Use this to avoid writing to stdout when it must stay
    /// reserved for another purpose, e.g. a stdio-based protocol such as MCP. See <see cref="AnsiConsoleFactory"/>
    /// for ready-made consoles that avoid stdout.
    /// </param>
    /// <param name="fileSystem">The file system abstraction to use. Defaults to the real file system.</param>
    public CSharpSolutionAnalysis(IAnsiConsole console, IFileSystem? fileSystem = null)
        : base(console, fileSystem ?? new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CSharpSolutionAnalysis"/> 
    /// </summary>
    /// <remarks>A typical simple call would be <![CDATA[await CsharpProjectAnalysis.Init.LoadAsync(..somepath)]]></remarks>
    public static CSharpSolutionAnalysis Init => new CSharpSolutionAnalysis();
    
    
    /// <summary>
    /// Initializes the MSBuild workspace and loads the C# project at the specified path.
    /// </summary>
    /// <param name="path">The absolute path to the <c>.csproj</c> file to load.</param>
    
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous load operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="BaseAnalysis{CSharpSolutionAnalysis}.Loaded"/> has already been called on this instance.
    /// </exception>
    /// <remarks>When this has been successfully loaded</remarks>
    protected internal override async Task InternalLoad(AbsolutePath path, 
        //bool loadCompilation,  
        CancellationToken cancellationToken = default)
    {
        ValidateAndInitializeWorkspace(path);

        InternalSolution = await Console.Status().StartAsync("[green]Loading solution[/]",
            async _ => await InternalMsBuildWorkspace.OpenSolutionAsync(path.Value,cancellationToken: cancellationToken));
        Loaded = true;
    }

    
    

    /// <inheritdoc />
    protected override void EnsureCorrectlyLoadedExtraChecks()
    {
        if (InternalSolution is null)
        {
            throw new InvalidOperationException("Solution must not be null");
        }
    }

    /// <summary>
    /// Gets or sets the internal Roslyn Solution instance that represents the loaded C# solution.
    /// </summary>
    /// <remarks>
    /// This property is null until the solution is successfully loaded via InternalLoad.
    /// It serves as the backing field for the public Solution property and is checked during EnsureCorrectlyLoadedExtraChecks validation.
    /// </remarks>
    private Solution? InternalSolution { get; set; }

    /// <summary>
    /// Gets the loaded C# solution. This property is only available after a successful call to <see cref="InternalLoad"/>.
    /// </summary>
    public Solution Solution
    {
        get
        {
            EnsureCorrectlyLoaded();
            return InternalSolution!;
        }
    }

   


    
}