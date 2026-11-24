#:package FileBasedApp.Toolkit@1.4.0
#:package FileBasedApp.Toolkit.SimpleExec@1.4.0

using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.SimpleExec;
using TruePath;

namespace Worktree.Utils;

/// <summary>
/// Describes a failure to query Git or interpret its output.
/// </summary>
/// <param name="ErrorText">The message to show to the caller.</param>
public sealed record Error(string ErrorText);

public readonly struct VoidResult;

/// <summary>
/// Holds either a successful value or an error.
/// </summary>
/// <typeparam name="T">The type of the successful value.</typeparam>
public union MatchOrError<T>(T, Error)
{
    /// <summary>
    /// Applies the next operation to a successful value or passes the original error through.
    /// </summary>
    /// <typeparam name="U">The success type of the next operation.</typeparam>
    /// <param name="next">The operation to apply on success.</param>
    public MatchOrError<U> Bind<U>(Func<T, MatchOrError<U>> next) => this switch
    {
        T success => next(success),
        Error error => error,
        _ => throw new UnreachableException()
    };
}

/// <summary>
/// Composes asynchronous operations that stop on the first error.
/// </summary>
public static class MatchOrErrorExtensions
{
    extension<T>(Task<MatchOrError<T>> match)
    {
        /// <summary>
        /// Runs the next fallible operation on success or passes the original error through.
        /// </summary>
        /// <typeparam name="U">The success type of the next operation.</typeparam>
        /// <param name="next">The asynchronous operation to apply on success.</param>
        public async Task<MatchOrError<U>> BindToMatchAsync<U>(Func<T, Task<MatchOrError<U>>> next)
        {
            var result = await match.ConfigureAwait(false);

            return result switch
            {
                T success => await next(success),
                Error error => error,
                _ => throw new UnreachableException()
            };
        }

        /// <summary>
        /// Wraps the next operation's value on success or passes the original error through.
        /// </summary>
        /// <typeparam name="U">The type of the value produced by the next operation.</typeparam>
        /// <param name="next">The asynchronous operation to apply on success.</param>
        public async Task<MatchOrError<U>> BindToValueAsync<U>(Func<T, Task<U>> next)
        {
            var result = await match.ConfigureAwait(false);

            return result switch
            {
                T success => await next(success),
                Error error => error,
                _ => throw new UnreachableException()
            };
        }
    }
}

/// <summary>
/// Queries Git for commit metadata used when listing worktrees.
/// </summary>
public static class GitUtil
{
    /// <summary>
    /// Perform git worktree prune
    /// </summary>
    /// <param name="rootPath"></param>
    public static async Task<MatchOrError<VoidResult>> Prune(AbsolutePath rootPath)
    {
        try
        {
            await SimpleExecRunner.Init("git").AddArguments("worktree", "prune").WithWorkingDirectory(rootPath)
                .RunAsync();
            return new VoidResult();
        }
        catch (Exception e)
        {
            return new Error(e.ToString());
        }
    }

    /// <summary>
    /// Reads the latest commit's committer timestamp in UTC or returns an error.
    /// </summary>
    /// <param name="path">The worktree directory to query.</param>
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
            DateTimeStyles.AdjustToUniversal, out var result)
            ? result
            : new Error($"Could not parse {readResult.StandardOutput}");
    }
}

/// <summary>
/// Locates Git worktrees and reads their branch and commit details.
/// </summary>
public static class WorktreeUtil
{
    /// <summary>
    /// Lists worktrees with branch information for the repository at the given path.
    /// </summary>
    /// <param name="match">The directory from which to run Git.</param>
    /// <param name="errorDisplayer">The callback for an empty listing or a commit timestamp error.</param>
    /// <param name="cancellationToken">The token used to cancel Git execution and output parsing.</param>
    public static async IAsyncEnumerable<Worktree> GetWorktrees(AbsolutePath match, Action<Error> errorDisplayer,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await SimpleExecRunner
            .Init("git").AddArguments("worktree", "list")
            .AddArgument("--porcelain")
            .WithWorkingDirectory(match)
            .WithNoEcho()
            .ReadAsync(token: cancellationToken);

        if (string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            errorDisplayer(new Error("No worktree found"));
            yield break;
        }

        await foreach (var worktree in ParseOutput(result.StandardOutput, errorDisplayer, cancellationToken))
        {
            yield return worktree;
        }
    }

    /// <summary>
    /// Parses Git's porcelain worktree records and attaches each worktree's latest commit timestamp.
    /// </summary>
    /// <param name="resultStandardOutput">The output of <c>git worktree list --porcelain</c>.</param>
    /// <param name="errorHandler">The callback for commit timestamp errors.</param>
    /// <param name="cancellationToken">The token used to cancel reading the output.</param>
    private static async IAsyncEnumerable<Worktree> ParseOutput(
        string resultStandardOutput,
        Action<Error> errorHandler,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stringReader = new StringReader(resultStandardOutput);

        var regex = new Regex("^worktree (?<path>.+)$");
        var head = new Regex("^HEAD (?<head>.+)$");
        var branch = new Regex("^branch refs/heads/(?<branch>.+)$");

        while (stringReader.Peek() > -1)
        {
            var currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            if (regex.Match(currentLine) is not {Success: true} match) continue;
            var path = AbsolutePath.Create(match.Groups["path"].Value);

            DateTimeOffset? HandleError(Error e)
            {
                errorHandler(e);
                return null;
            }

            DateTimeOffset? lastChange = await GitUtil.GetLastCommitTime(path) switch
            {
                DateTimeOffset d => d,
                Error e => HandleError(e)
            };

            currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            if (head.Match(currentLine) is not {Success: true} headMatch) continue;
            var headMatchValue = headMatch.Groups["head"].Value;
            currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            if (branch.Match(currentLine) is not {Success: true} branchMatch) continue;
            var branchMatchValue = branchMatch.Groups["branch"].Value;
            yield return new Worktree(path, branchMatchValue, headMatchValue, lastChange);
        }
    }

    /// <summary>
    /// Returns the first matching directory, checking the starting directory before its children.
    /// </summary>
    /// <param name="startPath">The directory to check and search.</param>
    public static async Task<MatchOrError<AbsolutePath>> GetWorktreePathsFromRoot(AbsolutePath startPath)
    {
        var results = await GetWorktreePaths([startPath, .. startPath.GetDirectories()]).ToListAsync();
        if (results.Count == 0)
        {
            return new Error("No worktree found");
        }

        return results[0];
    }

    /// <summary>
    /// Probes candidate directories in order and yields those from which Git can list worktrees.
    /// </summary>
    /// <param name="paths">The candidate directories to check.</param>
    private static async IAsyncEnumerable<AbsolutePath> GetWorktreePaths(IEnumerable<AbsolutePath> paths)
    {
        foreach (var absolutePath in paths)
        {
            if (await GetWorktreePathOrNull(absolutePath) is { } match)
            {
                yield return match;
            }
        }
    }

    /// <summary>
    /// Returns a candidate path if it passes the Git worktree probe, or <see langword="null"/> otherwise.
    /// </summary>
    /// <param name="source">The candidate directory.</param>
    internal static async Task<AbsolutePath?> GetWorktreePathOrNull(AbsolutePath source)
    {
        if (await TestIsWorktree(source))
        {
            return source;
        }

        return null;
    }

    /// <summary>
    /// Checks whether Git can list worktrees from the candidate directory without writing to standard error.
    /// </summary>
    /// <param name="source">The directory from which to run Git.</param>
    internal static async Task<bool> TestIsWorktree(AbsolutePath source)
    {
        var result = await SimpleExecRunner.Init("git")
            .AddArguments("worktree", "list")
            .WithNoEcho()
            .WithWorkingDirectory(source)
            .WithExitCodeHandler(_ => true)
            .ReadAsync();

        return string.IsNullOrWhiteSpace(result.StandardError);
    }
}

/// <summary>
/// Represents a Git worktree and its checked-out branch and commit.
/// </summary>
/// <param name="Path">The worktree directory.</param>
/// <param name="Name">The checked-out branch name.</param>
/// <param name="Head">The commit at the worktree's HEAD.</param>
/// <param name="LastCommit">The latest commit timestamp, if available.</param>
public record Worktree(AbsolutePath Path, string Name, string Head, DateTimeOffset? LastCommit)
{
    /// <summary>
    /// Defers checking whether the worktree has a <c>.git</c> directory rather than a <c>.git</c> file.
    /// </summary>
    public Lazy<bool> IsPrimaryLazy => new(() => (Path / ".git").DirectoryExists());

    public bool IsPrimary => IsPrimaryLazy.Value;

    /// <summary>
    /// Produces a descending sort key that puts the primary worktree first and undated worktrees last.
    /// </summary>
    /// <param name="arg">The worktree to order.</param>
    public static DateTimeOffset? Order(Worktree arg)
    {
        if (arg.IsPrimary)
        {
            return DateTimeOffset.MaxValue;
        }

        return arg.LastCommit ?? DateTimeOffset.MinValue;
    }
}