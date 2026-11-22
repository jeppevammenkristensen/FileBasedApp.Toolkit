using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.SimpleExec;
using Spectre.Console;
using TruePath;

namespace Worktree.Utils;

public static class WorktreeUtil
{
    public static async IAsyncEnumerable<Worktree> GetWorktrees(AbsolutePath match, Action<Error> errorDisplayer, [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
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

        await foreach (var worktree in ParseOutput(result.StandardOutput, errorDisplayer,cancellationToken))
        {
            yield return worktree;
        }
    }
    
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

            Error? error = null;

            DateTimeOffset? HandleError(Error e)
            {
                errorHandler(e);
                //AnsiConsole.MarkupLineInterpolated($"[red]Could not get last commit date for {path}. [bold]{error.ErrorText}[/][/]");
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

    public static async Task<MatchOrError<AbsolutePath>> GetWorktreePathsFromRoot(AbsolutePath startPath)
    {
       
        var results = await GetWorktreePaths([startPath, .. startPath.GetDirectories()]).ToListAsync();
        if (results.Count == 0)
        {
            return new Error("No worktree found");
        }
        
        return results[0];
    }
    
    // ReSharper disable once AsyncMethodWithoutAwait
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
    
    internal static async Task<AbsolutePath?> GetWorktreePathOrNull(AbsolutePath source)
    {
        if (await TestIsWorktree(source))
        {
            return source;
        }

        return null;
    }

    internal static async Task<bool> TestIsWorktree(AbsolutePath source)
    {
        var result =  await SimpleExecRunner.Init("git")
            .AddArguments("worktree", "list")
            .WithNoEcho()
            .WithWorkingDirectory(source)
            .WithExitCodeHandler(_ => true)
            .ReadAsync();

        return string.IsNullOrWhiteSpace(result.StandardError);
    }
}

public record Worktree(AbsolutePath Path, string Name, string Head, DateTimeOffset? LastCommit)
{
    public Lazy<bool> IsPrimaryLazy => new Lazy<bool>(() => (Path / ".git").DirectoryExists()); // if .git is a directory this is the root worktree

    public bool IsPrimary => IsPrimaryLazy.Value;
    
    public static DateTimeOffset? Order(Worktree arg)
    {
        if (arg.IsPrimary)
        {
            return DateTimeOffset.MaxValue;
        }

        return arg.LastCommit ?? DateTimeOffset.MinValue;
            
    }
}