#:package FileBasedApp.Toolkit@1.4.0
#:package FileBasedApp.Toolkit.Spectre@1.4.0
#:package FileBasedApp.Toolkit.SimpleExec@1.4.0
#:package TextCopy@6.2.1
#:property PublishAot=false
#:property PackAsTool=true
#:property ToolCommandName=worktree-path-selector
#:property PackageId=worktree-path-selector
#:property Version=1.1.0
using System.Globalization;
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.CommandCli;
  

var commandApp = new CommandApp<RunCommand>()
    .WithDescription("Presents a select prompt to navigate worktrees");

commandApp.Configure(ctx =>
{
    ctx.PropagateExceptions();
    ctx.UseAssemblyInformationalVersion();
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync)
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AbsolutePath[] candidatePaths = [settings.DirectoryAbsolute, ..settings.DirectoryAbsolute.GetDirectories()];

        var match = await GetWorktreePaths(candidatePaths).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (match == default)
        {
            AnsiConsole.MarkupLine("[red]No worktree candidate found[/]");
            return -1;
        }
        
        AnsiConsole.MarkupLineInterpolated($"[green]Found worktree candidate [bold]{match}[/][/]");

        var result = await SimpleExecRunner
            .Init("git").AddArguments("worktree", "list")
            .AddArgument("--porcelain")
            .WithWorkingDirectory(match)
            .WithNoEcho()
            .ReadAsync(token: cancellationToken);

        if (string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            AnsiConsole.MarkupLine("[red]No worktrees found[/]");
            return -1;
        }

        var parsedWorktrees = await ParseOutput(result.StandardOutput,
                cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        var promptResult = await AnsiConsole.PromptAsync(new SelectionPrompt<Worktree>()
            .EnableSearch()
            .Title("Select worktree")
            .AddChoices(parsedWorktrees.OrderByDescending(Worktree.Order))
            .UseConverter(WorktreeChoiceConverter), cancellationToken);
        
        AnsiConsole.MarkupLineInterpolated($"[green]Selected worktree [bold]{promptResult.Path.Value}[/][/]");
        await TextCopy.ClipboardService.SetTextAsync(promptResult.Path.Value, cancellationToken);
        return 0; // 0 for success
    }

    private static string WorktreeChoiceConverter(Worktree worktree)
    {
        return $"[bold]{worktree.Name.EscapeMarkup()}[/] {worktree.Path.Value.EscapeMarkup()} [dim]{worktree.Head.EscapeMarkup()} [green]{worktree.LastCommit}[/][/]";
    }

    public record Worktree(AbsolutePath Path, string Name, string Head, DateTimeOffset? LastCommit)
    {
        public static DateTimeOffset? Order(Worktree arg)
        {
            if (arg.Name == "main" || arg.Name == "master")
            {
                return DateTimeOffset.MaxValue;
            }

            return arg.LastCommit ?? DateTimeOffset.MinValue;
            
        }
    }

    private async IAsyncEnumerable<Worktree> ParseOutput(
        string resultStandardOutput,
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


            DateTimeOffset? HandleError(Error error)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Could not get last commit date for {path}. [bold]{error.ErrorText}[/][/]");
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

    // ReSharper disable once AsyncMethodWithoutAwait
    private async IAsyncEnumerable<AbsolutePath> GetWorktreePaths(IEnumerable<AbsolutePath> paths)
    {
        foreach (var absolutePath in paths)
        {
            if (await GetWorktreePathOrNull(absolutePath) is { } match)
            {
                yield return match;
            }
        }
    }
    
    private async Task<AbsolutePath?> GetWorktreePathOrNull(AbsolutePath source)
    {
        if (await TestIsWorktree(source))
        {
            return source;
        }

        return null;
    }

    private async Task<bool> TestIsWorktree(AbsolutePath source)
    {
        var result =  await SimpleExecRunner.Init("git")
            .AddArguments("worktree", "list")
            .WithNoEcho()
            .WithWorkingDirectory(source)
            .WithExitCodeHandler(_ => true)
            .ReadAsync();

        return string.IsNullOrWhiteSpace(result.StandardError);
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandArgument(0, "[Directory]")]
        public string? Directory { get; set; }
        /// <summary></summary>
        public AbsolutePath DirectoryAbsolute { get; private set; }

        protected override ValidationResult DoValidate()
        {
            // Exceptions here will bubble up and be output as validation errors.
            // Resolve an omitted or relative path from the directory where the command was invoked.
            
            // PredefinedRootPath.ExecutionFolder => The folder where this .cs file live in. Good if you scripts like (Run tests, Build something etc)
            // PredefinedRootPath.CurrentDirectory => The folder where this is executed from. Good for published scripts.
            
            DirectoryAbsolute = this.TryGetDirectory(Directory, allowEmpty: true, shouldExist: true, root: PredefinedRootPath.CurrentDirectory);
            return base.DoValidate();
        }
    }
}

public sealed record Error(string ErrorText);

public union MatchOrError<T>(T, Error);

public static class GitUtil
{
    public static async Task<MatchOrError<DateTimeOffset>> GetLastCommitTime(AbsolutePath path)
    {
        var (standardOutput, standardError) = await SimpleExecRunner.Init("git")
            .AddArgumentPair("-C", path) // Run from the given folder
            .AddArgumentPair("log", "-1") // Retrieve 1
            .AddArgument("--format=%cI")
            .ReadAsync();

        if (!string.IsNullOrWhiteSpace(standardError))
        {
            return new Error(standardError);
        }
        
        return DateTimeOffset.TryParse(
            standardOutput.TrimEnd("\n"),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal, out var result) ? result : new Error($"Could not parse {standardOutput}");
    }
}
