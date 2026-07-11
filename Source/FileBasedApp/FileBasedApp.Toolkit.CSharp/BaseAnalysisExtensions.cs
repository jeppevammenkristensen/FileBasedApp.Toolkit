using JetBrains.Annotations;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Extension methods for <see cref="BaseAnalysis{TSelf}"/> providing convenient shortcuts for
/// pointing console output somewhere other than stdout.
/// </summary>
[PublicAPI]
public static class BaseAnalysisExtensions
{
    extension<TSelf>(BaseAnalysis<TSelf> self) where TSelf : BaseAnalysis<TSelf>
    {
        /// <summary>
        /// Replaces the console with one that discards all output. Use this when no diagnostic output is
        /// wanted at all, e.g. behind a stdio-based protocol such as MCP where stdout must stay reserved
        /// for the protocol stream.
        /// </summary>
        /// <returns>The current instance to enable method chaining.</returns>
        public TSelf WithQuietConsole() => self.WithAnsiConsole(AnsiConsoleFactory.Quiet());

        /// <summary>
        /// Replaces the console with one that writes all output to stderr instead of stdout. Use this to keep
        /// diagnostic output visible while leaving stdout clean, e.g. behind a stdio-based protocol such as MCP.
        /// </summary>
        /// <returns>The current instance to enable method chaining.</returns>
        public TSelf WithStderrConsole() => self.WithAnsiConsole(AnsiConsoleFactory.Stderr());
    }
}
