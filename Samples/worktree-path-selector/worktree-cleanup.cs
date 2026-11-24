#!/usr/bin/env dotnet

#:package FileBasedApp.Toolkit@1.4.0
#:package FileBasedApp.Toolkit.SimpleExec@1.4.0
#:package FileBasedApp.Toolkit.Spectre@1.4.0
#:package AutoSpectre@0.16.0
#:include Shared.cs

#:property PublishAot=true
#:property PackAsTool=true
#:property ToolCommandName=worktree-path-cleanup
#:property PackageId=worktree-path-cleanup
#:property Version=1.4.0

using System.IO.Abstractions;
using AutoSpectre;
using FileBasedApp.Toolkit;
using Spectre.Console;
using Worktree.Utils;

var cts = new CancellationTokenSource();

void CancelHandler(object? _, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
}

try
{
    var cancellationToken = cts.Token;

    var directory = PathUtil.TryGetDirectory(args is {Length: 0} ? null : args[0], true, false,
        PredefinedRootPath.CurrentDirectory);

    var result = await WorktreeUtil.GetWorktreePathsFromRoot(directory)
        .BindToValueAsync(async x =>
            await WorktreeUtil.GetWorktrees(x, HandleError, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken))
        .BindToMatchAsync(async x =>
        {
            var cleanupPrompt = new CleanupPrompt(x);
            await cleanupPrompt.PromptAsync(cancellationToken);
            return cleanupPrompt.Result;
        });

    switch (result)
    {
        case Error error:
            AnsiConsole.MarkupLineInterpolated($"[red]{error.ErrorText}[/]");
            return -1;
        case VoidResult:
            return 0;
    }
}
catch (Exception e)
{
    AnsiConsole.MarkupLineInterpolated($"[red]{e.Message}[/]");
}
finally
{
    Console.CancelKeyPress -= CancelHandler;
    cts.Dispose();
}

return 0; // 0 for success

void HandleError(Error error)
{
    AnsiConsole.MarkupLineInterpolated($"[red]{error.ErrorText}[/]");
}


[AutoSpectreForm(Culture = "da-DK")]
public partial class CleanupPrompt
{
    public CleanupPrompt(List<Worktree.Utils.Worktree> worktrees)
    {
        AvailableWorktrees = [.. worktrees.Where(x => !x.IsPrimary).OrderBy(x => x.LastCommit ?? DateTime.MinValue)];
        Primary = worktrees.First(x => x.IsPrimary);
    }

    public Worktree.Utils.Worktree Primary { get; set; }

    public List<Worktree.Utils.Worktree> AvailableWorktrees { get; set; }

    [SelectPrompt(
        Source = nameof(AvailableWorktrees),
        Title = "Select worktrees to cleanup", Converter = nameof(SelectedWorktreesConverter))]
    public partial List<Worktree.Utils.Worktree> SelectedWorktrees { get; set; } = [];

    public MatchOrError<VoidResult> Result { get; private set; }

    [TextPrompt(Title = "[yellow]Confirm that you want to delete[/]")]
    public bool ConfirmDeletion { get; set; }

    public string? SelectedWorktreesConverter(Worktree.Utils.Worktree worktree)
    {
        return EscapeInterpolated(
            $"{worktree.Name} {worktree.Path.Value} [green]{(worktree.LastCommit?.ToString("yyyy-MM-dd HH:mm:ss", this.GetCultureInfo()) ?? "N/A")}[/]");
    }

    [Break(Condition = nameof(ConfirmDeletion), NegateCondition = true)]
    public void Break()
    {
    }

    [TaskStep]
    public async Task Process(IAnsiConsole console)
    {
        foreach (var (index, worktree) in SelectedWorktrees.Index())
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[green][bold]{index + 1} of {SelectedWorktrees.Count}[/] Removing {worktree.Name}[/]");

            try
            {
                AnsiConsole.Status().Start(EscapeInterpolated($"[green]Deleting directory {worktree.Path.Value}[/]"),
                    _ => { worktree.Path.DirectoryDelete(true); });
            }
            catch (Exception ex)
            {
                console.MarkupLineInterpolated($"[yellow]Could not delete {worktree.Path.Value}:{ex.Message}[/]");
            }
        }

        Result = await GitUtil.Prune(Primary.Path);
    }
}