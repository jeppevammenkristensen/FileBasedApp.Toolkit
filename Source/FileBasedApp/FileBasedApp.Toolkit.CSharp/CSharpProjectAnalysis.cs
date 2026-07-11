using Microsoft.CodeAnalysis;
using Spectre.Console;
using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Abstract base class for loading and initializing C# projects using MSBuild workspace.
/// Provides infrastructure for project loading with customizable console output and file system abstraction.
/// </summary>
/// <remarks>
/// This class encapsulates the initialization of MSBuild workspace and handles the loading of C# projects.
/// It registers MSBuild defaults and configures project properties during the load process.
/// The class supports dependency injection of console and file system instances for better testability.
/// </remarks>
public sealed class CSharpProjectAnalysis : BaseAnalysis<CSharpProjectAnalysis>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CSharpProjectAnalysis"/> using the default
    /// <see cref="Spectre.Console.AnsiConsole.Console"/> and a new <see cref="FileSystem"/> instance.
    /// </summary>
    public CSharpProjectAnalysis()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CSharpProjectAnalysis"/> using a custom <see cref="IAnsiConsole"/>
    /// and, optionally, a custom <see cref="IFileSystem"/>.
    /// </summary>
    /// <param name="console">
    /// The console to write status and diagnostic output to. Use this to avoid writing to stdout when it must stay
    /// reserved for another purpose, e.g. a stdio-based protocol such as MCP. See <see cref="AnsiConsoleFactory"/>
    /// for ready-made consoles that avoid stdout.
    /// </param>
    /// <param name="fileSystem">The file system abstraction to use. Defaults to the real file system.</param>
    public CSharpProjectAnalysis(IAnsiConsole console, IFileSystem? fileSystem = null)
        : base(console, fileSystem ?? new FileSystem())
    {
    }

    /// <summary>
    /// Initalises a new instance of <see cref="CSharpProjectAnalysis"/> 
    /// </summary>
    /// <remarks>A typical simple call would be <![CDATA[await CsharpProjectAnalysis.Init.LoadAsync(..somepath)]]></remarks>
    public static CSharpProjectAnalysis Init => new CSharpProjectAnalysis();

    /// <summary>
    /// Initializes the MSBuild workspace and loads the C# project at the specified path.
    /// </summary>
    /// <param name="path">The absolute path to the <c>.csproj</c> file to load.</param>
    
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous load operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="BaseAnalysis{CsharpProjectAnalysis}.Loaded"/> has already been called on this instance.
    /// </exception>
    /// <remarks>When this has been successfully loaded</remarks>
    protected internal override async Task InternalLoad(AbsolutePath path, 
        //bool loadCompilation,  
        CancellationToken cancellationToken = default)
    {
        ValidateAndInitializeWorkspace(path);
        
        InternalProject = await Console.Status().StartAsync("[green]Loading project[/]",
            async _ => await InternalMsBuildWorkspace.OpenProjectAsync(path.Value,cancellationToken: cancellationToken));
        
        Console.MarkupLineInterpolated($"[green]Getting compilation for project: {InternalProject.Name}[/]");
        InternalCompilation =  await Console.Status().StartAsync("[green]Loading compilation[/]", async _ => await InternalProject.GetCompilationAsync(cancellationToken));
        Loaded = true;
    }

    /// <summary>
    /// Gets or sets the Roslyn compilation produced from the loaded project.
    /// </summary>
    /// <value>
    /// A <see cref="InternalCompilation"/> instance, or <see langword="null"/> if the project has not yet been initialized.
    /// </value>
    private Compilation? InternalCompilation { get; set; }

    /// <summary>
    /// Gets or sets the Roslyn project loaded from the MSBuild workspace.
    /// </summary>
    /// <value>
    /// A <see cref="InternalProject"/> instance, or <see langword="null"/> if the project has not yet been initialized.
    /// </value>
    private Project? InternalProject { get; set; }


    /// <summary>
    /// Gets the loaded C# project representation from the MSBuild workspace.
    /// </summary>
    /// <value>
    /// A <see cref="Microsoft.CodeAnalysis.Project"/> instance representing the loaded C# project with its compilation and metadata.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the project has not been loaded yet via the <see cref="BaseAnalysis{CsharpProjectAnalysis}.Loaded"/> method, or when the internal compilation or project state is null.
    /// </exception>
    /// <remarks>
    /// This property ensures that the project and its compilation are properly initialized before returning.
    /// Access to this property requires that the <see cref="BaseAnalysis{CsharpProjectAnalysis}"/> method has been successfully called first.
    /// </remarks>
    public Project Project
    {
        get
        {
            EnsureCorrectlyLoaded();
            return InternalProject!;
        }
    }


    /// <summary>
    /// Gets the Roslyn compilation object representing the loaded C# project.
    /// </summary>
    /// <value>
    /// A <see cref="CompilationWrapper"/> instance that represents the compiled state of the project, including all syntax trees, references, and semantic information.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the project has not been loaded yet or when the compilation cannot be retrieved from the loaded project.
    /// </exception>
    public Compilation Compilation
    {
        get
        {
            EnsureCorrectlyLoaded();
            return InternalCompilation!;
        }
    }

    /// <summary>
    /// Returns the compilation wrapped. This contains specialized behavior for the compilation.
    /// For instance an ability to cache types requested
    /// </summary>
    public CompilationWrapper CompilationWrapper
    {
        get
        {
            field ??= new CompilationWrapper(Compilation);
            return field;
        }
    }


    /// <inheritdoc />
    protected override void EnsureCorrectlyLoadedExtraChecks()
    {
        if (InternalCompilation is null || InternalProject is null)
        {
            throw new InvalidOperationException("Compilation, Project must not be null");
        }
    }

}