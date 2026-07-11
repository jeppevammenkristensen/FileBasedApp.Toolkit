#:package FileBasedApp.Toolkit@0.21.0-alpha-03
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
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLineInterpolated($"[green]SomePath is {settings.SomePathAbsolute.Value}[/]");
        var directoryInfo = DirectoryInfoFactory.New(settings.SomePathAbsolute);
        AnsiConsole.MarkupLineInterpolated($"[dim]Creation time: {directoryInfo.CreationTimeUtc}[/]");
        settings.SomePathAbsolute.DirectoryGetParent()!.GetAbsolutePath();
        var parentDirectory = settings.SomePathAbsolute / "..";
        AnsiConsole.Status().Start("Checking for .cs files", ctx =>
        {
            // Uses the extensions method to GetFiles
            foreach (var csfile in parentDirectory.EnumerateFiles("*.cs", SearchOption.AllDirectories).Take(50))
            {
                AnsiConsole.MarkupLineInterpolated($"[dim]Relative path {csfile.RelativeTo(settings.SomePathAbsolute)}[/]");
            }
        });
        await new SimpleExecRunner("dotnet").AddArgument("--version").WithWorkingDirectory(parentDirectory).RunAsync(token: cancellationToken);
        return 0; // 0 for success
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandArgument(0, "[SomePath]")]
        public string? SomePath { get; set; }
        /// <summary></summary>
        public AbsolutePath SomePathAbsolute { get; private set; }

        protected override ValidationResult DoValidate()
        {
            // Exceptions here will bubble up and outputted as validation		
            // This will evaluate the path. If the path is relative, it will relative (in this case) against the execution folder. That would be the
            // directory that this .cs lives in
            SomePathAbsolute = this.TryGetDirectory(SomePath, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);
            return base.DoValidate();
        }
    }
}