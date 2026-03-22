using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;
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
public class CsharpProjectAnalysis : IDisposable, IAsyncDisposable
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Gets the console instance used for outputting diagnostic and status information during project operations.
    /// </summary>
    /// <value>
    /// An <see cref="IAnsiConsole"/> instance that provides methods for writing formatted output to the console.
    /// This console is used throughout the project loading process to display status updates and diagnostics.
    /// </value>
    protected IAnsiConsole Console { get; }

    
    public static CsharpProjectAnalysis Init => new CsharpProjectAnalysis();
    
    /// <summary>
    /// Initializes a new instance of <see cref="CsharpProjectAnalysis"/> using the default
    /// <see cref="AnsiConsole.Console"/> and a new <see cref="FileSystem"/> instance.
    /// </summary>
    protected internal CsharpProjectAnalysis() : this(AnsiConsole.Console, new FileSystem())
    {
    }

    /// <summary>
    /// Abstract base class for loading and initializing C# projects using MSBuild workspace.
    /// Provides infrastructure for project loading with customizable console output and file system abstraction.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the initialization of MSBuild workspace and handles the loading of C# projects.
    /// It registers MSBuild defaults and configures project properties during the load process.
    /// The class supports dependency injection of console and file system instances for better testability.
    /// Derived classes can leverage the loaded project and compilation for further analysis or processing.
    /// </remarks>
    protected internal CsharpProjectAnalysis(IAnsiConsole console, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        Console = console;
        Properties = ImmutableDictionary<string, string>.Empty;
        this.WithProperty("TargetFramework", DefaultTargetFramework);
    }

    /// <summary>
    /// Specifies the default target framework version used when loading C# projects.
    /// </summary>
    /// <value>
    /// A string constant representing the target framework identifier. The default value is "net10.0".
    /// This value is automatically assigned to the "TargetFramework" MSBuild property during loader initialization.
    /// </value>
    public const string  DefaultTargetFramework = "net10.0";

    /// <summary>
    /// Gets the MSBuild workspace used to load and interact with the C# project.
    /// </summary>
    /// <value>
    /// An <see cref="MSBuildWorkspace"/> instance created during <see cref="Loaded"/>.
    /// This property is only valid after the loader has been successfully initialized.
    /// </value>
    public MSBuildWorkspace? MsBuildWorkspace => EnsureCompilationAndProject().workspace;

    /// <summary>
    /// Gets or sets the collection of MSBuild properties that will be applied when loading the C# project.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableDictionary{TKey, TValue}"/> where keys are property names and values are property values.
    /// These properties are passed to MSBuild during project loading to configure the build environment and project settings.
    /// The dictionary is immutable and can be modified through extension methods like AddProperty.
    /// </value>
    /// <remarks>Use the AddProperty extension method to add properties.</remarks>
    public ImmutableDictionary<string, string> Properties { get; internal set; }

    /// <summary>
    /// Initializes the MSBuild workspace and loads the C# project at the specified path.
    /// </summary>
    /// <param name="path">The absolute path to the <c>.csproj</c> file to load.</param>
    /// <param name="loadCompilation"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous load operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="Loaded"/> has already been called on this instance.
    /// </exception>
    /// <remarks>When this has been successfully loaded</remarks>
    protected internal async Task InternalLoad(AbsolutePath path, 
        //bool loadCompilation,  
        CancellationToken cancellationToken = default)
    {
        if (!path.FileExists(_fileSystem))
        {
            throw new InvalidOperationException($"The file {path} does not exist");
        }
        
        if (Loaded)
        {
            throw new InvalidOperationException("The project has already been initialized");
        }

        try
        {
            MSBuildLocator.RegisterDefaults();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
       
        InternalMsBuildWorkspace = MSBuildWorkspace.Create(Properties);
        
        InternalProject = await Console.Status().StartAsync("[green]Loading project[/]",
            async _ => await InternalMsBuildWorkspace.OpenProjectAsync(path.Value,cancellationToken: cancellationToken));
        
        AnsiConsole.MarkupLineInterpolated($"[green]Getting compilation for project: {InternalProject.Name}[/]");
        InternalCompilation =  await Console.Status().StartAsync("[green]Loading compilation[/]", async _ => await InternalProject.GetCompilationAsync(cancellationToken));
        Loaded = true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Loaded"/> has been called successfully.
    /// </summary>
    protected bool Loaded { get; set; }

    /// <summary>
    /// Gets or sets the Roslyn compilation produced from the loaded project.
    /// </summary>
    /// <value>
    /// A <see cref="InternalCompilation"/> instance, or <see langword="null"/> if the project has not yet been initialized.
    /// </value>
    private Compilation? InternalCompilation { get; set; }
    
    private MSBuildWorkspace? InternalMsBuildWorkspace { get; set; }

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
    /// Thrown when the project has not been loaded yet via the <see cref="Loaded"/> method, or when the internal compilation or project state is null.
    /// </exception>
    /// <remarks>
    /// This property ensures that the project and its compilation are properly initialized before returning.
    /// Access to this property requires that the <see cref="Loaded"/> method has been successfully called first.
    /// </remarks>
    public Project Project => EnsureCompilationAndProject().project;

    /// <summary>
    /// Gets the MSBuild workspace instance used for managing and loading the C# project.
    /// </summary>
    /// <value>
    /// An <see cref="MSBuildWorkspace"/> instance that represents the loaded workspace containing the project.
    /// This property ensures that the project has been properly loaded before returning the workspace instance.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the project has not been loaded yet or when the workspace initialization has failed.
    /// </exception>
    public MSBuildWorkspace Workspace => EnsureCompilationAndProject().workspace;


    /// <summary>
    /// Gets the Roslyn compilation object representing the loaded C# project.
    /// </summary>
    /// <value>
    /// A <see cref="Compilation"/> instance that represents the compiled state of the project, including all syntax trees, references, and semantic information.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the project has not been loaded yet or when the compilation cannot be retrieved from the loaded project.
    /// </exception>
    public Compilation Compilation => EnsureCompilationAndProject().compilation;

    /// <summary>
    /// Ensures that both <see cref="InternalCompilation"/> and <see cref="InternalProject"/> are available,
    /// throwing if the loader has not been initialized yet.
    /// </summary>
    /// <returns>
    /// A tuple containing the non-null <see cref="InternalCompilation"/> and <see cref="InternalProject"/> instances.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="Loaded"/> has not been called before accessing compilation or project data.
    /// </exception>
    protected (Compilation compilation, Project project, MSBuildWorkspace workspace) EnsureCompilationAndProject()
    {
        if (!Loaded)
        {
            throw new InvalidOperationException(
                "The Csharp project loader has not been initalized yet. Call Initialize to load");
        }
        
        if (InternalCompilation is null || InternalProject is null || InternalMsBuildWorkspace is null)
        {
            throw new InvalidOperationException("Compilation and Project must not be null");
        }

        return (InternalCompilation!, InternalProject!, InternalMsBuildWorkspace!);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="CsharpProjectAnalysis"/>.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if called from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            MsBuildWorkspace?.Dispose();
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="CsharpProjectAnalysis"/> instance.
    /// This method calls the protected Dispose method with disposing set to true and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases all resources used by the <see cref="CsharpProjectAnalysis"/>.
    /// This method performs synchronous disposal and suppresses finalization.
    /// </summary>
    /// <return>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</return>
    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}