using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;


/// <summary>
/// Base class for C# project analysis
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public abstract class BaseAnalysis<TSelf> : IDisposable, IAsyncDisposable where TSelf : BaseAnalysis<TSelf>
{
    /// <summary>
    /// Gets the file system abstraction used for file and directory operations throughout the analysis process.
    /// </summary>
    /// <value>
    /// An <see cref="IFileSystem"/> instance that provides an abstraction layer for file system operations,
    /// enabling testability and cross-platform compatibility. This file system is used to verify file existence
    /// and perform other file-related operations during project analysis.
    /// </value>
    protected readonly IFileSystem FileSystem;

    /// <summary>
    /// Specifies the default target framework version used when loading C# projects.
    /// </summary>
    /// <value>
    /// A string constant representing the target framework identifier. The default value is "net10.0".
    /// This value is automatically assigned to the "TargetFramework" MSBuild property during loader initialization.
    /// </value>
    public const string  DefaultTargetFramework = "net10.0";
    
    /// <summary>
    /// Gets the console instance used for outputting diagnostic and status information during project operations.
    /// </summary>
    /// <value>
    /// An <see cref="IAnsiConsole"/> instance that provides methods for writing formatted output to the console.
    /// This console is used throughout the project loading process to display status updates and diagnostics.
    /// </value>
    protected IAnsiConsole Console { get; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="CsharpProjectAnalysis"/> using the default
    /// <see cref="AnsiConsole.Console"/> and a new <see cref="FileSystem"/> instance.
    /// </summary>
    protected internal BaseAnalysis() : this(AnsiConsole.Console, new FileSystem())
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TSelf> Load(AbsolutePath path, CancellationToken cancellationToken = default)
    {
        await InternalLoad(path, cancellationToken);
        return (TSelf) this;
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
    protected internal BaseAnalysis(IAnsiConsole console, IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
        Console = console;
        Properties = ImmutableDictionary<string, string>.Empty;
        WithProperty("TargetFramework", DefaultTargetFramework);
    }
    
    /// <summary>
    /// Gets or sets the collection of MSBuild properties that will be applied when loading the C# project.
    /// </summary>
    /// <value>
    /// An <see cref="ImmutableDictionary{TKey, TValue}"/> where keys are property names and values are property values.
    /// These properties are passed to MSBuild during project loading to configure the build environment and project settings.
    /// The dictionary is immutable and can be modified through extension methods like AddProperty.
    /// </value>
    /// <remarks>Use the AddProperty extension method to add properties.</remarks>
    public ImmutableDictionary<string, string> Properties { get; protected set; }

    /// <summary>
    /// Gets or sets the MSBuild workspace instance used to load and interact with MSBuild-based projects and solutions.
    /// </summary>
    /// <value>
    /// An <see cref="MSBuildWorkspace"/> instance that provides access to the underlying MSBuild workspace.
    /// The getter ensures that the workspace is correctly loaded before returning the instance by calling
    /// <see cref="EnsureCorrectlyLoaded"/>. Returns <see langword="null"/> if the workspace has not been initialized.
    /// </value>
    /// <remarks>
    /// This property is automatically initialized during the load process when opening projects or solutions.
    /// It serves as the core component for interacting with the Roslyn MSBuild workspace API, enabling
    /// project and solution loading, compilation access, and code analysis operations.
    /// </remarks>
    protected MSBuildWorkspace? InternalMsBuildWorkspace { get; set; }

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
    public MSBuildWorkspace Workspace
    {
        get
        {
            EnsureCorrectlyLoaded();
            return InternalMsBuildWorkspace!;
        }
    } 

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Loaded"/> has been called successfully.
    /// </summary>
    protected bool Loaded { get; set; }

    /// <summary>
    /// Sets or updates a project property with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <return>The current instance to enable method chaining.</return>
    public TSelf WithProperty(string name, string value)
    {
        Properties = Properties.SetItem(name, value);
        return (TSelf)this;
    }

    /// <summary>
    /// Initializes the MSBuild workspace and loads the C# project at the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected internal abstract Task InternalLoad(AbsolutePath path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected void EnsureCorrectlyLoaded()
    {
        if (!Loaded)
        {
            throw new InvalidOperationException(
                "The Csharp project loader has not been initalized yet. Call Initialize to load");
        }

        if (InternalMsBuildWorkspace is null)
        {
            throw new InvalidOperationException("MsBuildWorkspace must not be null");
        }

        EnsureCorrectlyLoadedExtraChecks();
    }

    /// <summary>
    /// Performs additional checks to ensure that the loader is correctly loaded. This method is called after verifying that the loader has been initialized and the MSBuild workspace is available.
    /// </summary>
    protected abstract void EnsureCorrectlyLoadedExtraChecks();
    
    /// <summary>
    /// Validates the specified path and initializes the MSBuild workspace for loading the C# project.
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="InvalidOperationException"></exception>
    [MemberNotNull(nameof(InternalMsBuildWorkspace))]
    protected void ValidateAndInitializeWorkspace(AbsolutePath path)
    {
        if (!path.FileExists(FileSystem))
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
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="BaseAnalysis{TSelf}"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            InternalMsBuildWorkspace?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Asynchronously releases the unmanaged resources used by the <see cref="BaseAnalysis{TSelf}"/>
    /// and optionally releases the managed resources.
    /// </summary>
    /// <return>
    /// A <see cref="ValueTask"/> that represents the asynchronous dispose operation.
    /// </return>
    protected ValueTask DisposeAsyncCore()
    {
        InternalMsBuildWorkspace?.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
} 

/// <summary>
/// Abstract base class for loading and initializing C# projects using MSBuild workspace.
/// Provides infrastructure for project loading with customizable console output and file system abstraction.
/// </summary>
/// <remarks>
/// This class encapsulates the initialization of MSBuild workspace and handles the loading of C# projects.
/// It registers MSBuild defaults and configures project properties during the load process.
/// The class supports dependency injection of console and file system instances for better testability.
/// </remarks>
public sealed class CsharpProjectAnalysis : BaseAnalysis<CsharpProjectAnalysis>
{
    
    /// <summary>
    /// Initalises a new instance of <see cref="CsharpProjectAnalysis"/> 
    /// </summary>
    /// <remarks>A typical simple call would be <![CDATA[await CsharpProjectAnalysis.Init.LoadAsync(..somepath)]]></remarks>
    public static CsharpProjectAnalysis Init => new CsharpProjectAnalysis();

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