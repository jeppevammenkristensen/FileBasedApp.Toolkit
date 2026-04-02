#:package FileBasedApp.Toolkit.CSharp@0.18.0-alpha-11
#:package Dumpify@*

#:property PublishAot=false 

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
using System.IO.Abstractions;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.CSharp;
using Dumpify;

	
var commandApp = new CommandApp<RunCommand>()
	.WithDescription("Enter the description here");

commandApp.Configure(ctx => {	
	ctx.PropagateExceptions();
});
	
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{	
		AnsiConsole.MarkupLineInterpolated($"[green]RootPath is {settings.RootPathAbsolute.Value}[/]");
		
		foreach (var csFile in settings.RootPathAbsolute.EnumerateAllFiles("*.cs"))
		{	
			if (await csFile.IsFileBasedAppAsync(token:cancellationToken))
			{			
				AnsiConsole.MarkupLineInterpolated($"{csFile.FileName}");
				
				var fileBasedWrapper = new FileBasedAppWrapper(csFile);
				foreach (var item in fileBasedWrapper.PackageDirectives.Where(x => x.PackageInfo?.Name == "FileBasedApp.Toolkit"))
				{
					if (item.PackageInfo!.Version.Value != "0.17.1-alpha-02")
					{
						item.PackageInfo.Version.Value = "0.17.1-alpha-02";
						var compilationUnitSyntax = item.Update(fileBasedWrapper.CompilationUnitSyntax);		
						Console.WriteLine(compilationUnitSyntax.ToFullString());
						fileBasedWrapper.CompilationUnitSyntax = compilationUnitSyntax;
						//fileBasedWrapper.Save();
						"Saved".Dump();
					}
					item.PackageInfo.DumpConsole();
				}
				//fileBasedWrapper.Path.Value.DumpConsole();
			}
		}
		
		return 0; // 0 for success
	}

	public class Settings : ExtendedCommandSettings
	{
		[CommandArgument(0, "[RootFolder]")]
		public string? RootFolder { get; set; }

		/// <summary></summary>
		public AbsolutePath RootPathAbsolute { get; private set; }
		protected override ValidationResult DoValidate()
		{
			// Exceptions here will bubble up and outputted as validation		

			// This will evaluate the path. If the path is relative, it will relative (in this case) against the execution folder. That would be the
			// directory that this .cs lives in
			RootPathAbsolute = this.TryGetDirectory(RootFolder, allowEmpty: true, shouldExist: true, PredefinedRootPath.ExecutionFolder);         
    
			return base.DoValidate();
		}
	}
}