#:package FileBasedApp.Toolkit@1.4.0
#:package FileBasedApp.Toolkit.SimpleExec@1.4.0
#:package FileBasedApp.Toolkit.Spectre@1.4.0
#:package AutoSpectre@0.15.0
#:project Worktree.Utils/Worktree.Utils.csproj

#:property PublishAot=false
#:property PackAsTool=true
#:property ToolCommandName=worktree-path-cleanup
#:property PackageId=worktree-path-cleanup
#:property Version=1.1.0

using System.Globalization;
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using System.IO.Abstractions;
using AutoSpectre;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.SimpleExec;
using Worktree.Utils;

var commandApp = new CommandApp<RunCommand>().WithDescription("Cleanup worktree");
commandApp.Configure(ctx =>
{
    ctx.UseAssemblyInformationalVersion();
    ctx.PropagateExceptions();
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync)
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = await WorktreeUtil.GetWorktreePathsFromRoot(settings.DirectoryAbsolute)
            .BindToValueAsync(async x =>
                await WorktreeUtil.GetWorktrees(x, HandleError, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken: cancellationToken))
            .BindToMatchAsync(async x =>
            {
                var cleanupPrompt = new Prompt.CleanupPrompt(x);
                await cleanupPrompt.PromptAsync(cancellationToken);
                return cleanupPrompt.Result;
            });

        switch (result)
        {
            case Error error:
                AnsiConsole.MarkupLineInterpolated($"[red]{error.ErrorText}[/]");
                return -1;
            case int matchOrError:
                return matchOrError;
        }
        
        return 0; // 0 for success
    }
    
    private void HandleError(Error error)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]{error.ErrorText}[/]");
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
            // This will evaluate the path. If the path is relative, it will be resolved relative to the execution folder (Controlled by the root parameter). That would be the
            // directory that this .cs lives in 
            
            // PredefinedRootPath.ExecutionFolder => The folder where this .cs file live in. Good if you scripts like (Run tests, Build something etc)
            // PredefinedRootPath.CurrentDirectory => The folder where this is executed from. Good for published scripts.
            
            DirectoryAbsolute = this.TryGetDirectory(Directory, allowEmpty: true, shouldExist: true, root: PredefinedRootPath.ExecutionFolder);
            return base.DoValidate();
        }
    }
}

namespace Prompt
{
    [AutoSpectreForm(Culture = "da-DK")] 
    public partial class CleanupPrompt
    {
        public CleanupPrompt(List<Worktree.Utils.Worktree> worktrees)
        {
            AvailableWorktrees = [.. worktrees.Where(x => !x.IsPrimary)];
            Primary = worktrees.First(x => x.IsPrimary);
        }

        public Worktree.Utils.Worktree Primary { get; set; }

        public List<Worktree.Utils.Worktree> AvailableWorktrees { get; set; }

        [SelectPrompt(
            Source = nameof(AvailableWorktrees),
            Title = "Select worktrees to cleanup", Converter = nameof(SelectedWorktreesConverter))]
        public partial List<Worktree.Utils.Worktree> SelectedWorktrees { get; set; } = [];

        public string? SelectedWorktreesConverter(Worktree.Utils.Worktree worktree)
        {
            var culture = new CultureInfo("da-DK");
            return $"{worktree.Name.EscapeMarkup()} {worktree.Path.Value.EscapeMarkup()} [green]{(worktree.LastCommit?.ToString("yyyy-MM-dd HH:mm:ss",culture) ?? "N/A").EscapeMarkup()}[/]";
        }

        public MatchOrError<int> Result { get; private set; } = 0;

        [TaskStep]
        public async System.Threading.Tasks.Task Process(IAnsiConsole console)
        {
            foreach (var worktree in SelectedWorktrees)
            {
                AnsiConsole.MarkupLineInterpolated($"[green]Removing {worktree.Name}[/]");
                
                try
                {
                    worktree.Path.DirectoryDelete(true);
                }
                catch (Exception ex)
                {
                    console.MarkupLineInterpolated($"[yellow]Could not delete {worktree.Path.Value}:{ex.Message}[/]");
                }
            }

            try
            {
                
                await SimpleExecRunner.Init("git").AddArguments("worktree", "prune").WithWorkingDirectory(Primary.Path).RunAsync();
            }
            catch (Exception e)
            {
                Result = new Error(e.ToString());
            }
        }
    }
}
