#:package FileBasedApp.Toolkit@1.1.1
#:package TextCopy@6.2.1
#:property PublishAot=false
#:property PackAsTool=true
#:property ToolCommandName=worktree-path-selector
#:property PackageId=worktree-path-selector
#:property Version=1.0.1
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit.CommandCli;
  

var commandApp = new CommandApp<RunCommand>().WithDescription("Presents a select prompt to navigate worktrees");
commandApp.Configure(ctx =>
{
    ctx.PropagateExceptions();
    ctx.UseAssemblyInformationalVersion();
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync)
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AbsolutePath[] candidatePaths = [settings.DirectoryAbsolute, ..settings.DirectoryAbsolute.GetDirectories()];

        var match = await GetWorktreePaths(candidatePaths).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (match == default)
        {
            AnsiConsole.MarkupLine("[red]No worktree candidate found[/]");
            return -1;
        }
        
        AnsiConsole.MarkupLineInterpolated($"[green]Found worktree candidate [bold]{match}[/][/]");

        var result = await SimpleExecRunner.Init("git").AddArguments("worktree", "list").AddArgument("--porcelain")
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
            .AddChoices(parsedWorktrees.OrderBy(Worktree.Order))
            .UseConverter(x =>
                $"[bold]{x.Name.EscapeMarkup()}[/] {x.Path.Value.EscapeMarkup()} [dim]{x.Head.EscapeMarkup()}[/]"), cancellationToken);
        
        AnsiConsole.MarkupLineInterpolated($"[green]Selected worktree [bold]{promptResult.Path.Value}[/][/]");
        await TextCopy.ClipboardService.SetTextAsync(promptResult.Path.Value, cancellationToken);
        return 0; // 0 for success
    }
    
    public record Worktree(AbsolutePath Path, string Name, string Head)
    {
        public static AbsolutePath Order(Worktree arg)
        {
            if (arg.Name == "main" || arg.Name == "master")
            {
                return default;
            }

            return arg.Path;
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
            
            currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            if (head.Match(currentLine) is not {Success: true} headMatch) continue;
            var headMatchValue = headMatch.Groups["head"].Value;
            currentLine = await stringReader.ReadLineAsync(cancellationToken) ?? string.Empty;
            
            if (branch.Match(currentLine) is not {Success: true} branchMatch) continue;
            var branchMatchValue = branchMatch.Groups["branch"].Value;
            yield return new Worktree(path, branchMatchValue, headMatchValue);
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
