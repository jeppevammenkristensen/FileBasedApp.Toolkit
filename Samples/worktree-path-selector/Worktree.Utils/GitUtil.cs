using System.Globalization;
using System.IO.Abstractions;
using FileBasedApp.Toolkit.SimpleExec;
using TruePath;

namespace Worktree.Utils;

public static class GitUtil
{
    public static async Task<MatchOrError<DateTimeOffset>> GetLastCommitTime(AbsolutePath path)
    {
        if (!path.DirectoryExists())
        {
            return new Error($"Path {path} does not exist");
        }

        SimpleExecReadResult readResult;
        
        try
        {
            readResult = await SimpleExecRunner.Init("git")
                .AddArgumentPair("-C", path) // Run from the given folder
                .AddArgumentPair("log", "-1") // Retrieve 1
                .AddArgument("--format=%cI")
                .ReadEnhancedAsync();
        }
        catch (Exception e)
        {
            return new Error($"Error retrieving last {e.ToString()} for {path.Value}");
        }

        if (!string.IsNullOrWhiteSpace(readResult.StandardError))
        {
            return new Error(readResult.StandardError);
        }
        
        return DateTimeOffset.TryParse(
            readResult.StandardOutput.TrimEnd("\n"),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal, out var result) ? result : new Error($"Could not parse {readResult.StandardOutput}");
    }
}