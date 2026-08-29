using System.Text;
using SimpleExec;
using Ct = System.Threading.CancellationToken;


namespace FileBasedApp.Toolkit.SimpleExec;

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

    private static SimpleExecCommand GetDefault()
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



