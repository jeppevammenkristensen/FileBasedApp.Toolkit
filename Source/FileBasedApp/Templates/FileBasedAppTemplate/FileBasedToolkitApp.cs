#:package FileBasedApp.Toolkit@1.1.1
#:property PublishAot=false 
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.IO.Abstractions;
using TruePath.TestableIO.System.IO;
using FileBasedApp.Toolkit.CommandCli;

var commandApp = new CommandApp<RunCommand>().WithDescription("Enter the description here");
commandApp.Configure(ctx =>
{
#if DEBUG
	ctx.PropagateExceptions();	
#endif
    ctx.UseAssemblyInformationalVersion();
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync)
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLineInterpolated($"[green]Directory is {settings.DirectoryAbsolute.Value}[/]");
        var directoryInfo = DirectoryInfoFactory.New(settings.DirectoryAbsolute);
        AnsiConsole.MarkupLineInterpolated($"[dim]Creation time: {directoryInfo.CreationTimeUtc}[/]");
        settings.DirectoryAbsolute.DirectoryGetParent()!.GetAbsolutePath();
        var parentDirectory = settings.DirectoryAbsolute / "..";
        AnsiConsole.Status().Start("Checking for .cs files", ctx =>
        {
            // Uses the extension method to GetFiles
            foreach (var csfile in parentDirectory.EnumerateFiles("*.cs", SearchOption.AllDirectories).Take(50))
            {
                AnsiConsole.MarkupLineInterpolated($"[dim]Relative path {csfile.RelativeTo(settings.DirectoryAbsolute)}[/]");
            }
        });
        await new SimpleExecRunner("dotnet").AddArgument("--version").WithWorkingDirectory(parentDirectory).RunAsync(token: cancellationToken);
        return 0; // 0 for success
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
