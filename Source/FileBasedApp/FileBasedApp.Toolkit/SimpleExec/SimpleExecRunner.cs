using System.Collections.Immutable;
using System.Text;
using TruePath;

namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// A fluent builder for constructing and executing system commands via the SimpleExec library.
/// Accumulates the command name, arguments, secrets, and working directory, then executes
/// the command through an <see cref="ISimpleExecCommandWrapper"/>.
/// </summary>
public class SimpleExecRunner
{
    private ISimpleExecCommandWrapper? _testWrapper;

    /// <summary>
    /// For unit testing only. Allows for simple injection of the <see cref="ISimpleExecCommandWrapper"/> wrapper.
    /// </summary>
    /// <param name="simpleExecCommandWrapper">The wrapper to use.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    internal SimpleExecRunner WithSimpleExecWrapper(ISimpleExecCommandWrapper simpleExecCommandWrapper)
    {
        _testWrapper = simpleExecCommandWrapper;
        return this;
    }

    private ISimpleExecCommandWrapper Wrapper => _testWrapper ?? SimpleExecCommand.Instance;
    
    /// <summary>
    /// The name of the command to execute. For instance git
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Arguments for the command. Can be set with AddArgument
    /// </summary>
    public ImmutableArray<string> Arguments { get; private set; } = [];
    
    /// <summary>
    /// Secrets for the command
    /// </summary>
    /// <remarks>Only relevant if calling Run or RunAsync</remarks>
    public ImmutableArray<string> Secrets { get; private set; } = [];

    /// <summary>
    /// The working directory in which the command will be executed.
    /// When <see langword="null"/>, the current process working directory is used.
    /// </summary>
    protected AbsolutePath? WorkingDirectory { get; private set; }

    /// <summary>
    /// An optional action that configures environment variables for the command execution.
    /// This delegate receives the current environment variable dictionary and can add, modify, or remove variables before the command runs.
    /// </summary>
    protected Action<IDictionary<string, string?>>? ConfigureEnvironment { get; private set; }

    /// <summary>
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <see langword="true"/>, when the command is cancelled, any child processes created
    /// by the command are left running after the command is cancelled.
    /// </summary>
    protected bool CancellationIgnoresProcessTree { get; private set; }

    /// <summary>
    /// Whether to run the command without creating a new window.
    /// </summary>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    protected bool CreateNoWindow { get; private set; }

    ///<inheritdoc cref="SimpleExecRunner.Init"/>
    public SimpleExecRunner(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="SimpleExecRunner"/> with the specified command name.
    /// </summary>
    /// <param name="name">The name of the command to run. For instance dotnet, git, etc...</param>
    /// <returns></returns>
    /// <remarks>Use chaining methods like .Add... or .With... to add properties. End with a call to Run, RunAsync, or ReadAsync</remarks>
    public static SimpleExecRunner Init(string name) => new(name);

    /// <summary>
    /// Sets the working directory in which the command will be executed.
    /// </summary>
    /// <param name="workingDirectory">The absolute path to the working directory.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    public SimpleExecRunner WithWorkingDirectory(AbsolutePath workingDirectory)
    {
        WorkingDirectory = workingDirectory;
        return this;
    }

    /// <summary>
    /// Appends a single argument to the command's argument list.
    /// </summary>
    /// <param name="argument">The argument string to append.</param>
    /// <param name="isSecret">
    /// When <see langword="true"/>, the argument is also added to <see cref="Secrets"/>
    /// so it is redacted from any echoed output.
    /// </param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>isSecret is only effective if calling Run or RunAsync</remarks>
    public SimpleExecRunner AddArgument(string argument, bool isSecret = false)
    {
        ArgumentNullException.ThrowIfNull(argument);

        Arguments = Arguments.Add(argument);
        if (isSecret)
        {
            Secrets = Secrets.Add(argument);    
        }
        
        return this;
    }

    /// <summary>
    /// Appends the string representation of an <see cref="AbsolutePath"/> as a single argument.
    /// </summary>
    /// <param name="path">The absolute path to append as an argument.</param>
    /// <param name="isSecret">When <see langword="true"/>, the path value is treated as a secret.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    public SimpleExecRunner AddArgument(AbsolutePath path, bool isSecret = false) => AddArgument(path.Value, isSecret);

    /// <summary>
    /// Appends the string representation of a <see cref="LocalPath"/> as a single argument.
    /// </summary>
    /// <param name="path">The local path to append as an argument.</param>
    /// <param name="isSecret">When <see langword="true"/>, the path value is treated as a secret.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    public SimpleExecRunner AddArgument(LocalPath path, bool isSecret = false) => AddArgument(path.Value, isSecret);

    /// <summary>
    /// Appends a named argument together with its value as two separate arguments (e.g. <c>--flag value</c>).
    /// </summary>
    /// <param name="argument">The argument name or flag (e.g. <c>--output</c>).</param>
    /// <param name="value">The value that follows the argument name.</param>
    /// <param name="isSecret">When <see langword="true"/>, <paramref name="value"/> is treated as a secret.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>Only the value will be secret</remarks>
    public SimpleExecRunner AddArgumentPair(string argument, string value, bool isSecret = false) => AddArgument(argument).AddArgument(value, isSecret);

    /// <summary>
    /// Appends a named argument together with an <see cref="AbsolutePath"/> value.
    /// </summary>
    /// <param name="argument">The argument name or flag.</param>
    /// <param name="value">The absolute path value that follows the argument name.</param>
    /// <param name="isSecret">When <see langword="true"/>, the path value is treated as a secret.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    public SimpleExecRunner AddArgumentPair(string argument, AbsolutePath value, bool isSecret = false) => AddArgumentPair(argument, value.Value, isSecret);

    /// <summary>
    /// Appends a named argument together with a <see cref="LocalPath"/> value.
    /// </summary>
    /// <param name="argument">The argument name or flag.</param>
    /// <param name="value">The local path value that follows the argument name.</param>
    /// <param name="isSecret">When <see langword="true"/>, the path value is treated as a secret.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>isSecret is only relevant to set if you call Run or RunAsync</remarks>
    public SimpleExecRunner AddArgumentPair(string argument, LocalPath value, bool isSecret = false) => AddArgumentPair(argument, value.Value, isSecret);

    /// <summary>
    /// Appends multiple arguments to the command's argument list in a single call.
    /// </summary>
    /// <param name="arguments">The arguments to append.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>isSecret is only relevant to set if you call Run or RunAsync</remarks>
    public SimpleExecRunner AddArguments(params string[] arguments)
    {
        return AddArguments(false, arguments);
    }

    /// <summary>
    /// Appends multiple arguments to the command's argument list in a single call.
    /// </summary>
    /// <param name="isSecret">When <see langword="true"/>, every argument is treated as a secret.</param>
    /// <param name="arguments">The arguments to append.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>isSecret is only relevant to set if you call Run or RunAsync</remarks>
    public SimpleExecRunner AddArguments(bool isSecret, params string[] arguments)
    {
        foreach (var argument in arguments)
        {
            AddArgument(argument, isSecret);
        }

        return this;
    }


    /// <summary>
    /// Appends multiple values as secrets. The secrets must already have been defined as an argument to have an effect. if strict is true an exception will be thrown if unmatched secrets are passed
    /// </summary>
    /// <param name="secrets">The secrets to add</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if strict is true and unmatched secrets are used</exception>
    /// <remarks>Secrets are only relevant if calling Run or RunAsync</remarks>
    public SimpleExecRunner AddSecrets(params string[] secrets)
    {
        return AddSecrets(false, secrets);
    }

    /// <summary>
    /// Appends multiple values as secrets. The secrets must already have been defined as an argument to have an effect. if strict is true an exception will be thrown if unmatched secrets are passed
    /// </summary>
    /// <param name="strict">Evaluate the secrets against the existing arguments</param>
    /// <param name="secrets">The secrets to add</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if strict is true and unmatched secrets are used</exception>
    /// <remarks>Secrets are only relevant if calling Run or RunAsync</remarks>
    public SimpleExecRunner AddSecrets(bool strict, params string[] secrets)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (secrets.Any(x => x == null))
        {
            throw new ArgumentException("One or more secrets where null");
        }
        
        if (strict)
        {
            var unmatchedSecrets = Secrets.Except(secrets, StringComparer.OrdinalIgnoreCase);
            throw new InvalidOperationException("The following secrets were not found in the command: " + unmatchedSecrets.StringJoin(","));
        }
        
        Secrets = Secrets.AddRange(secrets);
        return this;
    }
    
    /// <summary>
    /// Sets whether cancellation should ignore the process tree.
    /// When enabled, child processes created by the command will continue running after cancellation.
    /// </summary>
    /// <param name="value">
    /// <see langword="true"/> to leave child processes running on cancellation;
    /// <see langword="false"/> (default) to cancel the entire process tree.
    /// </param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    public SimpleExecRunner WithCancellationIgnoresProcessTree(bool value = true)
    {
        CancellationIgnoresProcessTree = value;
        return this;
    }

    /// <summary>
    /// Configures environment variables for the command execution by providing an action that modifies the environment dictionary.
    /// </summary>
    /// <param name="action">An action that receives a dictionary of environment variables and their values, allowing modification before command execution.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    public SimpleExecRunner WithConfigureEnvironment(Action<IDictionary<string, string?>> action)
    {
        ConfigureEnvironment = action;
        return this;
    }

    /// <summary>
    /// Sets whether the command should run without creating a new window.
    /// </summary>
    /// <param name="value"><see langword="true"/> to suppress window creation; <see langword="false"/> otherwise.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    public SimpleExecRunner WithCreateNoWindow(bool value)
    {
        CreateNoWindow = value;
        return this;
    }

    /// <summary>
    /// Sets a custom handler to determine whether a command's exit code should be considered successful.
    /// The handler receives the exit code and returns true if the exit code is acceptable, false otherwise.
    /// </summary>
    /// <param name="handler">A function that receives an exit code and returns true if it represents success, false if it represents failure.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>If this is left out the default exit code handler logic is used.</remarks>
    public SimpleExecRunner WithExitCodeHandler(Func<int, bool> handler)
    {
        ExitCodeHandler = handler;
        return this;
    }

    /// <summary>
    /// Gets or sets a function that determines whether a given exit code should be considered successful.
    /// When set, the handler receives the process exit code and returns true if the exit code is acceptable,
    /// or false if it should be treated as an error.
    /// If null, the default behavior treats only exit code 0 as successful.
    /// </summary>
    public Func<int, bool>? ExitCodeHandler { get; private set; }

    /// <summary>
    /// The prefix to use when echoing the command line and working directory to standard output.
    /// When <see langword="null"/>, the default prefix is used.
    /// </summary>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    public string? EchoPrefix { get; private set; }

    /// <summary>
    /// Whether to suppress echoing the command line and working directory to standard output.
    /// Defaults to <see langword="false"/>, meaning the command is echoed.
    /// </summary>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    public bool NoEcho { get; private set; }

    /// <summary>
    /// The preferred encoding for standard output and standard error when reading command output.
    /// When <see langword="null"/>, the default encoding is used.
    /// </summary>
    /// <remarks>Only used by <see cref="ReadAsync"/>.</remarks>
    public Encoding? Encoding { get; private set; }

    /// <summary>
    /// Sets the preferred encoding for standard output and standard error.
    /// </summary>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>Only used by <see cref="ReadAsync"/>.</remarks>
    public SimpleExecRunner WithEncoding(Encoding encoding)
    {
        Encoding = encoding;
        return this;
    }

    /// <summary>
    /// The contents to write to standard input before the command runs.
    /// When <see langword="null"/>, no input is written.
    /// </summary>
    /// <remarks>Only used by <see cref="ReadAsync"/>.</remarks>
    public string? StandardInput { get; private set; }

    /// <summary>
    /// Sets the contents to write to standard input.
    /// </summary>
    /// <param name="standardInput">The string to write to stdin.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>Only used by <see cref="ReadAsync"/>.</remarks>
    public SimpleExecRunner WithStandardInput(string standardInput)
    {
        StandardInput = standardInput;
        return this;
    }

    /// <summary>
    /// Sets whether to suppress echoing the command line and working directory to standard output.
    /// </summary>
    /// <param name="value"><see langword="true"/> to suppress echoing; <see langword="false"/> to echo (default).</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    public SimpleExecRunner WithNoEcho(bool value = true)
    {
        NoEcho = value;
        return this;
    }

    /// <summary>
    /// Sets the prefix used when echoing the command line and working directory to standard output.
    /// </summary>
    /// <param name="echoPrefix">The prefix string to prepend when echoing.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for method chaining.</returns>
    /// <remarks>Only used by <see cref="Run"/> and <see cref="RunAsync"/>.</remarks>
    public SimpleExecRunner WithEchoPrefix(string echoPrefix)
    {
        EchoPrefix = echoPrefix;
        return this;
    }

    /// <summary>
    /// Executes the command synchronously using the configured name, arguments, and working directory.
    /// </summary>
    /// <param name="commandWrapper">An optional <see cref="ISimpleExecCommandWrapper"/> to use for execution. When <see langword="null"/>, the internally configured wrapper is used.</param>
    /// <param name="token">A cancellation token to observe while waiting for the command to exit.</param>
    public void Run(ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        (commandWrapper ?? Wrapper).Run(
            Name,
            Arguments,
            configureEnvironment: ConfigureEnvironment,
            workingDirectory: WorkingDirectory?.Value ?? string.Empty,
            cancellationIgnoresProcessTree: CancellationIgnoresProcessTree,
            createNoWindow: CreateNoWindow,
            secrets: Secrets,
            handleExitCode: ExitCodeHandler,
            echoPrefix: EchoPrefix,
            noEcho: NoEcho, ct: token);
    }

    /// <summary>
    /// Executes the command asynchronously with the configured arguments and options.
    /// </summary>
    /// <param name="commandWrapper">An optional <see cref="ISimpleExecCommandWrapper"/> to use for execution. When <see langword="null"/>, the internally configured wrapper is used.</param>
    /// <param name="token">A cancellation token to observe while waiting for the command to exit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RunAsync(ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        return (commandWrapper ?? Wrapper).RunAsync(Name,
            Arguments,
            configureEnvironment: ConfigureEnvironment,
            workingDirectory: WorkingDirectory?.Value ?? string.Empty,
            cancellationIgnoresProcessTree: CancellationIgnoresProcessTree,
            createNoWindow: CreateNoWindow,
            secrets: Secrets,
            handleExitCode: ExitCodeHandler,
            echoPrefix: EchoPrefix,
            noEcho: NoEcho, ct: token);
    }

    /// <summary>
    /// Executes the command asynchronously and captures the standard output and standard error streams as strings.
    /// </summary>
    /// <param name="commandWrapper">An optional <see cref="ISimpleExecCommandWrapper"/> to use for execution. When <see langword="null"/>, the internally configured wrapper is used.</param>
    /// <param name="token">A cancellation token to observe while waiting for the command to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the captured standard output and standard error.</returns>
    public Task<(string StandardOutput, string StandardError)> ReadAsync(
        ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        return (commandWrapper ?? Wrapper).ReadAsync(Name,
            Arguments,
            configureEnvironment: ConfigureEnvironment,
            workingDirectory: WorkingDirectory?.Value ?? string.Empty,
            cancellationIgnoresProcessTree: CancellationIgnoresProcessTree,
            handleExitCode: ExitCodeHandler,
            encoding: Encoding,
            standardInput: StandardInput,
            ct: token);
    }
}