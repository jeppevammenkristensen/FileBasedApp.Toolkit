using Spectre.Console;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Creates <see cref="IAnsiConsole"/> instances that avoid writing to stdout, for use when stdout must stay
/// reserved for another purpose, e.g. a stdio-based protocol such as MCP.
/// </summary>
public static class AnsiConsoleFactory
{
    /// <summary>
    /// Creates an <see cref="IAnsiConsole"/> that writes all output to stderr instead of stdout.
    /// </summary>
    public static IAnsiConsole Stderr() => AnsiConsole.Create(new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput(Console.Error)
    });

    /// <summary>
    /// Creates an <see cref="IAnsiConsole"/> that discards all output.
    /// </summary>
    public static IAnsiConsole Quiet() => AnsiConsole.Create(new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput(TextWriter.Null)
    });
}
