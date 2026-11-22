#:package FileBasedApp.Toolkit@1.4.0
#:package FileBasedApp.Toolkit.Spectre@1.4.0
#:package FileBasedApp.Toolkit.SimpleExec@1.4.0
#:package TextCopy@6.2.1
#:project Worktree.Utils/Worktree.Utils.csproj
#:property PublishAot=false
#:property PackAsTool=true
#:property ToolCommandName=worktree-path-selector
#:property PackageId=worktree-path-selector
#:property Version=1.1.0
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.CommandCli;
using Worktree.Utils;
  

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

        AbsolutePath root = default;
        
        
        void HandleError(Error e)
        {
            AnsiConsole.MarkupLine($"[red]{e}[/]");
        }
        
        var result = await WorktreeUtil.GetWorktreePathsFromRoot(settings.DirectoryAbsolute)
            .BindToValueAsync(async x =>
            {
                AnsiConsole.MarkupLineInterpolated($"[green]Found worktree candidate [bold]{root}[/][/]");
                return await WorktreeUtil.GetWorktrees(x, HandleError, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken);
            })
            .BindToValueAsync(async x =>
            {
                var promptResult = await AnsiConsole.PromptAsync(new SelectionPrompt<Worktree.Utils.Worktree>()
                    .EnableSearch()
                    .Title("Select worktree")
                    .AddChoices(x.OrderByDescending(Worktree.Utils.Worktree.Order))
                    .UseConverter(WorktreeChoiceConverter), cancellationToken);
                AnsiConsole.MarkupLineInterpolated($"[green]Selected worktree [bold]{promptResult.Path.Value}[/][/]");
                return promptResult.Path;
            });

        if (result is Error error)
        {
            HandleError(error);
            return -1;
        }

        if (result is AbsolutePath match)
        {
            AnsiConsole.MarkupLineInterpolated($"[green]Selected worktree [bold]{match}[/][/]");
            await TextCopy.ClipboardService.SetTextAsync(match.Value, cancellationToken);
            return 0; // 0 for success
        }

        return -1;

    }

    private static string WorktreeChoiceConverter(Worktree.Utils.Worktree worktree)
    {
        return $"[bold]{worktree.Name.EscapeMarkup()}[/] {worktree.Path.Value.EscapeMarkup()} [dim]{worktree.Head.EscapeMarkup()} [green]{worktree.LastCommit}[/][/]";
    }
   

    // private async IAsyncEnumerable<Worktree> ParseOutput(
    //     string resultStandardOutput,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     using var stringReader = new StringReader(resultStandardOutput);
    //
    //     var regex = new Regex("^worktree (?<path>.+)$");
    //     var head = new Regex("^HEAD (?<head>.+)$");
    //     var branch = new Regex("^branch refs/heads/(?<branch>.+)$");
    //     
    //     while (stringReader.Peek() > -1)
    //     {
    //         var currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;
    //         
    //         if (regex.Match(currentLine) is not {Success: true} match) continue;
    //         var path = AbsolutePath.Create(match.Groups["path"].Value);
    //
    //         Error? error = null;
    //
    //
    //         DateTimeOffset? HandleError(Error error)
    //         {
    //             AnsiConsole.MarkupLineInterpolated($"[red]Could not get last commit date for {path}. [bold]{error.ErrorText}[/][/]");
    //             return null;
    //         }
    //
    //         DateTimeOffset? lastChange = await GitUtil.GetLastCommitTime(path) switch
    //         {
    //             DateTimeOffset d => d,
    //             Error e => HandleError(e)
    //         };
    //         
    //         currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;
    //
    //         if (head.Match(currentLine) is not {Success: true} headMatch) continue;
    //         var headMatchValue = headMatch.Groups["head"].Value;
    //         currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;
    //         
    //         if (branch.Match(currentLine) is not {Success: true} branchMatch) continue;
    //         var branchMatchValue = branchMatch.Groups["branch"].Value;
    //         yield return new Worktree(path, branchMatchValue, headMatchValue, lastChange);
    //     }
    // }

    

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


