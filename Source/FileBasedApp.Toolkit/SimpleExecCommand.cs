using System.Collections.Immutable;using System.Text;
using SimpleExec;
using TruePath;
using Ct = System.Threading.CancellationToken;


namespace FileBasedApp.Toolkit;

/// <summary>
/// A fluent builder for constructing and executing system commands via the SimpleExec library.
/// Accumulates the command name, arguments, secrets, and working directory, then executes
/// the command through an <see cref="ISimpleExecCommandWrapper"/>.
/// </summary>
public class SimpleExecRunner
{
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

    /// <summary>
    /// Initializes a new instance of <see cref="SimpleExecRunner"/> with the specified command name.
    /// </summary>
    /// <param name="name">The name of the command to execute, e.g. <c>git</c>.</param>
    public SimpleExecRunner(string name)
    {
        Name = name;
    }

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
    public SimpleExecRunner AddArgumentPair(string argument, string value, bool isSecret) => AddArgument($"{argument} {value}").AddArgument(value, isSecret);

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
    /// <param name="isSecret">When <see langword="true"/>, every argument is treated as a secret.</param>
    /// <param name="arguments">The arguments to append.</param>
    /// <returns>The current <see cref="SimpleExecRunner"/> instance for chaining.</returns>
    /// <remarks>isSecret is only relevant to set if you call Run or RunAsync</remarks>
    public SimpleExecRunner AddArguments(bool isSecret = false, params string[] arguments)
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
    /// <param name="strict">Evaluate the secrets against the existing arguments</param>
    /// <param name="secrets">The secrets to add</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if strict is true and unmatched secrets are used</exception>
    /// <remarks>Secrets are only relevant if calling Run or RunAsync</remarks>
    public SimpleExecRunner AddSecrets(bool strict = false, params string[] secrets)
    {
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
    /// <param name="commandWrapper">
    /// An optional <see cref="ISimpleExecCommandWrapper"/> to use for execution.
    /// Defaults to <see cref="SimpleExecCommand.Instance"/> when <see langword="null"/>.
    /// </param>
    /// <param name="token"></param>
    public void Run(ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        commandWrapper ??= SimpleExecCommand.Instance;
        commandWrapper.Run(
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
    /// <param name="commandWrapper">The command wrapper to use for execution. If null, the default SimpleExecCommand.Instance will be used.</param>
    /// <param name="token">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RunAsync(ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        commandWrapper ??= SimpleExecCommand.Instance;
        return commandWrapper.RunAsync(Name,
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
    /// <param name="commandWrapper">An optional command wrapper to execute the command. If <see langword="null"/>, the default implementation is used.</param>
    /// <param name="token">A cancellation token to observe while waiting for the command to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the captured standard output and standard error.</returns>
    public Task<(string StandardOutput, string StandardError)> ReadAsync(
        ISimpleExecCommandWrapper? commandWrapper = null, CancellationToken token = default)
    {
        commandWrapper ??= SimpleExecCommand.Instance;
        return commandWrapper.ReadAsync(Name,
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

// This has been generated with help from the wrapper.cs file in the samples with the source code for the Command class in the clipboard
// and with the parameter --className SimpleExecCommand

/// <summary>
/// A class that provides functionality to execute system commands
/// leveraging the capabilities of the SimpleExec library.
/// </summary>
public class SimpleExecCommand : ISimpleExecCommandWrapper,
    IStaticValueSetter<ISimpleExecCommandWrapper>
{
    /// <summary>
    /// Gets or sets the current instance of the object.
    /// This property provides a reference to the active instance,
    /// enabling access to shared or singleton behavior.
    /// </summary>
    public static ISimpleExecCommandWrapper Instance { get; private set; } = GetDefault();
    
    /// <summary>
    /// Runs a command without redirecting standard output (stdout) and standard error (stderr) and without writing to standard input (stdin).
    /// By default, the command line is echoed to standard output (stdout).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">The arguments to pass to the command.</param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "secrets">A list of secrets that are redacted by replacement with "***" when echoing the resulting command line and the working directory (if specified) to standard output (stdout).</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "echoPrefix">The prefix to use when echoing the command line and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "noEcho">Whether to echo the resulting command line and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "createNoWindow">Whether to run the command in a new window.</param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <exception cref = "ExitCodeException">The command exited with non-zero exit code.</exception>
    /// <remarks>
    /// By default, the resulting command line and the working directory (if specified) are echoed to standard output (stdout).
    /// To suppress this behavior, provide the <paramref name = "noEcho"/> parameter with a value of <c>true</c>.
    /// </remarks>
    public void Run(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, Ct ct = default) => Command.Run(name, args, workingDirectory, configureEnvironment, secrets, handleExitCode, echoPrefix, noEcho, cancellationIgnoresProcessTree, createNoWindow, ct);
    /// <summary>
    /// Runs a command without redirecting standard output (stdout) and standard error (stderr) and without writing to standard input (stdin).
    /// By default, the command line is echoed to standard output (stdout).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">
    /// The arguments to pass to the command.
    /// As with <see cref = "System.Diagnostics.ProcessStartInfo.ArgumentList"/>, the strings don't need to be escaped.
    /// </param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "secrets">A list of secrets that are redacted by replacement with "***" when echoing the resulting command line and the working directory (if specified) to standard output (stdout).</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "echoPrefix">The prefix to use when echoing the command name, arguments, and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "noEcho">Whether to echo the resulting command name, arguments, and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "createNoWindow">Whether to run the command in a new window.</param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <exception cref = "ExitCodeException">The command exited with non-zero exit code.</exception>
    public void Run(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, Ct ct = default) => Command.Run(name, args, workingDirectory, configureEnvironment, secrets, handleExitCode, echoPrefix, noEcho, cancellationIgnoresProcessTree, createNoWindow, ct);
    /// <summary>
    /// Runs a command asynchronously without redirecting standard output (stdout) and standard error (stderr) and without writing to standard input (stdin).
    /// By default, the command line is echoed to standard output (stdout).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">The arguments to pass to the command.</param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "secrets">A list of secrets that are redacted by replacement with "***" when echoing the resulting command line and the working directory (if specified) to standard output (stdout).</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "echoPrefix">The prefix to use when echoing the command line and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "noEcho">Whether to echo the resulting command line and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "createNoWindow">Whether to run the command in a new window.</param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <returns>A <see cref = "Task"/> that represents the asynchronous running of the command.</returns>
    /// <exception cref = "ExitCodeReadException">The command exited with non-zero exit code.</exception>
    /// <remarks>
    /// By default, the resulting command line and the working directory (if specified) are echoed to standard output (stdout).
    /// To suppress this behavior, provide the <paramref name = "noEcho"/> parameter with a value of <c>true</c>.
    /// </remarks>
    public Task RunAsync(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, Ct ct = default) => Command.RunAsync(name, args, workingDirectory, configureEnvironment, secrets, handleExitCode, echoPrefix, noEcho, cancellationIgnoresProcessTree, createNoWindow, ct);
    /// <summary>
    /// Runs a command asynchronously without redirecting standard output (stdout) and standard error (stderr) and without writing to standard input (stdin).
    /// By default, the command line is echoed to standard output (stdout).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">
    /// The arguments to pass to the command.
    /// As with <see cref = "System.Diagnostics.ProcessStartInfo.ArgumentList"/>, the strings don't need to be escaped.
    /// </param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "secrets">A list of secrets that are redacted by replacement with "***" when echoing the resulting command line and the working directory (if specified) to standard output (stdout).</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "echoPrefix">The prefix to use when echoing the command name, arguments, and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "noEcho">Whether to echo the resulting command name, arguments, and working directory (if specified) to standard output (stdout).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "createNoWindow">Whether to run the command in a new window.</param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <returns>A <see cref = "Task"/> that represents the asynchronous running of the command.</returns>
    /// <exception cref = "ExitCodeReadException">The command exited with non-zero exit code.</exception>
    public Task RunAsync(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, Ct ct = default) => Command.RunAsync(name, args, workingDirectory, configureEnvironment, secrets, handleExitCode, echoPrefix, noEcho, cancellationIgnoresProcessTree, createNoWindow, ct);
    /// <summary>
    /// Runs a command and reads standard output (stdout) and standard error (stderr) and optionally writes to standard input (stdin).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">The arguments to pass to the command.</param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "encoding">The preferred <see cref = "Encoding"/> for standard output (stdout) and standard output (stdout).</param>
    /// <param name = "standardInput">The contents of standard input (stdin).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <returns>
    /// A <see cref = "Task{TResult}"/> representing the asynchronous running of the command and reading of standard output (stdout) and standard error (stderr).
    /// The task result is a <see cref = "ValueTuple{T1, T2}"/> representing the contents of standard output (stdout) and standard error (stderr).
    /// </returns>
    /// <exception cref = "ExitCodeReadException">
    /// The command exited with non-zero exit code. The exception contains the contents of standard output (stdout) and standard error (stderr).
    /// </exception>
    public Task<(string StandardOutput, string StandardError)> ReadAsync(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, Func<int, bool>? handleExitCode = null, Encoding? encoding = null, string? standardInput = null, bool cancellationIgnoresProcessTree = false, Ct ct = default) => Command.ReadAsync(name, args, workingDirectory, configureEnvironment, handleExitCode, encoding, standardInput, cancellationIgnoresProcessTree, ct);
    /// <summary>
    /// Runs a command and reads standard output (stdout) and standard error (stderr) and optionally writes to standard input (stdin).
    /// </summary>
    /// <param name = "name">The name of the command. This can be a path to an executable file.</param>
    /// <param name = "args">
    /// The arguments to pass to the command.
    /// As with <see cref = "System.Diagnostics.ProcessStartInfo.ArgumentList"/>, the strings don't need to be escaped.
    /// </param>
    /// <param name = "workingDirectory">The working directory in which to run the command.</param>
    /// <param name = "configureEnvironment">An action which configures environment variables for the command.</param>
    /// <param name = "handleExitCode">
    /// A delegate which accepts an <see cref = "int "/> representing exit code of the command and
    /// returns <see langword="true"/> when it has handled the exit code and default exit code handling should be suppressed, and
    /// returns <see langword="false"/> otherwise.
    /// </param>
    /// <param name = "encoding">The preferred <see cref = "Encoding"/> for standard output (stdout) and standard error (stderr).</param>
    /// <param name = "standardInput">The contents of standard input (stdin).</param>
    /// <param name = "cancellationIgnoresProcessTree">
    /// Whether to ignore the process tree when cancelling the command.
    /// If set to <c>true</c>, when the command is cancelled, any child processes created by the command
    /// are left running after the command is cancelled.
    /// </param>
    /// <param name = "ct">A <see cref = "Ct"/> to observe while waiting for the command to exit.</param>
    /// <returns>
    /// A <see cref = "Task{TResult}"/> representing the asynchronous running of the command and reading of standard output (stdout) and standard error (stderr).
    /// The task result is a <see cref = "ValueTuple{T1, T2}"/> representing the contents of standard output (stdout) and standard error (stderr).
    /// </returns>
    /// <exception cref = "ExitCodeReadException">
    /// The command exited with non-zero exit code. The exception contains the contents of standard output (stdout) and standard error (stderr).
    /// </exception>
    public Task<(string StandardOutput, string StandardError)> ReadAsync(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, Func<int, bool>? handleExitCode = null, Encoding? encoding = null, string? standardInput = null, bool cancellationIgnoresProcessTree = false, Ct ct = default) => Command.ReadAsync(name, args, workingDirectory, configureEnvironment, handleExitCode, encoding, standardInput, cancellationIgnoresProcessTree, ct);

    private static ISimpleExecCommandWrapper GetDefault()
    {
        return new SimpleExecCommand();
    }
    
    /// <summary>
    /// The default instance
    /// </summary>
    /// <returns></returns>
    static ISimpleExecCommandWrapper IStaticValueSetter<ISimpleExecCommandWrapper>.GetDefault()
    {
        return GetDefault();
    }

    static ISimpleExecCommandWrapper IStaticValueSetter<ISimpleExecCommandWrapper>.GetFileSystem()
    {
        return Instance;
    }

    static void IStaticValueSetter<ISimpleExecCommandWrapper>.SetFileSystem(ISimpleExecCommandWrapper fileSystem)
    {
       Instance = fileSystem;
    }
}



