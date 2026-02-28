using System.IO.Abstractions;
using FileBasedApp.Toolkit;
using GetFileSystemSourceAndParse;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;
using Spectre.Console.Cli;
using TruePath;

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;

    public RunCommand() : this(new FileSystem(), AnsiConsole.Console)
    {
    }

    internal RunCommand(IFileSystem fileSystem, IAnsiConsole console)
    {
        _fileSystem = fileSystem;
        _console = console;
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandOption("-p|--path")] public required string Path { get; set; }

        [CommandOption("--skip-fetch")] public bool SkipFetch { get; set; }


        protected override ValidationResult DoValidate()
        {
            PathFull = this.TryGetDirectory(Path, PredefinedRootPath.CurrentDirectory, true, false);

            return base.DoValidate();
        }

        public AbsolutePath PathFull { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        _console.MarkupLineInterpolated($"[green]{settings.PathFull.Value}[/]");
        _fileSystem.Directory.CreateDirectory(settings.PathFull.Value);

        if (!settings.SkipFetch)
        {
            _console.MarkupLineInterpolated($"[green]Creating {settings.PathFull.Value}[/]");
            await SimpleExec.Command.RunAsync("git", ["clone", "https://github.com/Testably/Testably.Abstractions.git"],
                settings.PathFull.Value, ct: cancellationToken);
        }

        var codePath = settings.PathFull / "Testably.Abstractions" / "Source" /
                       "Testably.Abstractions.FileSystem.Interface" /
                       "Testably.Abstractions.FileSystem.Interface.csproj";

        Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

        var properties = new Dictionary<string, string>
        {
            // This overrides the project property during load
            ["TargetFramework"] = "net10.0"
        };
        
        var workspace = MSBuildWorkspace.Create(properties);
        
        

        
        var project = await _console.Status().StartAsync("Loading project",
            async ctx =>
            {
                return await workspace.OpenProjectAsync(codePath.Value,cancellationToken: cancellationToken);
            });

        
        
        
        
        
        ProjectProcessor processor = new();

        var compilation = await project.GetCompilationAsync(cancellationToken);
        
        ArgumentNullException.ThrowIfNull(compilation);

        var interfaces = new List<INamedTypeSymbol>();
        
        var result = new Stack<INamespaceOrTypeSymbol>();
        result.Push(compilation.Assembly.GlobalNamespace);

        while (result.Count > 0)
        {
            var current = result.Pop();

            foreach (var symbol in current.GetMembers())
            {
                if (symbol.Kind == SymbolKind.Namespace)
                {
                    result.Push((INamespaceOrTypeSymbol) symbol);
                }
                else
                {
                    if (symbol.Kind == SymbolKind.NamedType && symbol is INamedTypeSymbol { TypeKind: TypeKind.Interface} namedType )
                    {
                        interfaces.Add(namedType);
                    }
                }
            }
        }

        while (true)
        {
            var namedTypeSymbol = AnsiConsole.Prompt(new SelectionPrompt<INamedTypeSymbol>()
                .Title("Select interface to process")
                .AddChoices(interfaces)
                .UseConverter(x => x.Name));

            try
            {
                await processor.Process(project, compilation, namedTypeSymbol, cancellationToken);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
    }


}