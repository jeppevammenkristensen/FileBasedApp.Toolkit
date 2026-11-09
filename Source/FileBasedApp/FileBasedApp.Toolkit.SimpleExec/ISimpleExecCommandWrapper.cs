using System.Text;
using SimpleExec;

namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// An interface for wrapping a <see cref="Command"/> 
/// </summary>
public interface ISimpleExecCommandWrapper
{
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <exception cref = "ExitCodeException">The command exited with non-zero exit code.</exception>
    /// <remarks>
    /// By default, the resulting command line and the working directory (if specified) are echoed to standard output (stdout).
    /// To suppress this behavior, provide the <paramref name = "noEcho"/> parameter with a value of <c>true</c>.
    /// </remarks>
    void Run(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, CancellationToken ct = default);
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <exception cref = "ExitCodeException">The command exited with non-zero exit code.</exception>
    void Run(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, CancellationToken ct = default);
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <returns>A <see cref = "Task"/> that represents the asynchronous running of the command.</returns>
    /// <exception cref = "ExitCodeReadException">The command exited with non-zero exit code.</exception>
    /// <remarks>
    /// By default, the resulting command line and the working directory (if specified) are echoed to standard output (stdout).
    /// To suppress this behavior, provide the <paramref name = "noEcho"/> parameter with a value of <c>true</c>.
    /// </remarks>
    Task RunAsync(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, CancellationToken ct = default);
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <returns>A <see cref = "Task"/> that represents the asynchronous running of the command.</returns>
    /// <exception cref = "ExitCodeReadException">The command exited with non-zero exit code.</exception>
    Task RunAsync(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, IEnumerable<string>? secrets = null, Func<int, bool>? handleExitCode = null, string? echoPrefix = null, bool noEcho = false, bool cancellationIgnoresProcessTree = false, bool createNoWindow = false, CancellationToken ct = default);
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <returns>
    /// A <see cref = "Task{TResult}"/> representing the asynchronous running of the command and reading of standard output (stdout) and standard error (stderr).
    /// The task result is a <see cref = "ValueTuple{T1, T2}"/> representing the contents of standard output (stdout) and standard error (stderr).
    /// </returns>
    /// <exception cref = "ExitCodeReadException">
    /// The command exited with non-zero exit code. The exception contains the contents of standard output (stdout) and standard error (stderr).
    /// </exception>
    Task<(string StandardOutput, string StandardError)> ReadAsync(string name, string args = "", string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, Func<int, bool>? handleExitCode = null, Encoding? encoding = null, string? standardInput = null, bool cancellationIgnoresProcessTree = false, CancellationToken ct = default);
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
    /// <param name = "ct">A <see cref = "CancellationToken"/> to observe while waiting for the command to exit.</param>
    /// <returns>
    /// A <see cref = "Task{TResult}"/> representing the asynchronous running of the command and reading of standard output (stdout) and standard error (stderr).
    /// The task result is a <see cref = "ValueTuple{T1, T2}"/> representing the contents of standard output (stdout) and standard error (stderr).
    /// </returns>
    /// <exception cref = "ExitCodeReadException">
    /// The command exited with non-zero exit code. The exception contains the contents of standard output (stdout) and standard error (stderr).
    /// </exception>
    Task<(string StandardOutput, string StandardError)> ReadAsync(string name, IEnumerable<string> args, string workingDirectory = "", Action<IDictionary<string, string?>>? configureEnvironment = null, Func<int, bool>? handleExitCode = null, Encoding? encoding = null, string? standardInput = null, bool cancellationIgnoresProcessTree = false, CancellationToken ct = default);
}