#:package FileBasedApp.Toolkit.CSharp@1.0.0-alpha
#:package FileBasedApp.Toolkit.Dotnet@1.0.0
#:package Dumpify@*

#:property PublishAot=false

using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.CSharp;
using FileBasedApp.Toolkit.Dotnet;


var commandApp = new CommandApp<RunCommand>()
	.WithDescription("Enter the description here");

commandApp.Configure(ctx => {	
	ctx.PropagateExceptions();
});
	
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{	
		AnsiConsole.MarkupLineInterpolated($"[green] Traversing [bold]{settings.RootPathAbsolute.Value}[/] looking for filebased-apps[/]");
		
		Dictionary<string, string> NameVersion = new Dictionary<string, string>();
		var result =await DotnetRecipes.GetPackageInformation("FileBasedApp.Toolkit", true);

		void AddVersion(string name)
		{
			var highestVersion = result.GetHighestVersion(name);
			AnsiConsole.MarkupLineInterpolated($"[blue]Adding version for {name} {highestVersion.LatestVersion}[/]");
			NameVersion.Add(name, highestVersion.LatestVersion);	
		}
		
		foreach (var csFile in settings.RootPathAbsolute.EnumerateAllFiles("*.cs"))
		{	
			if (await csFile.IsFileBasedAppAsync(token:cancellationToken))
			{			
				AnsiConsole.Write(new Rule(csFile.FileName).LeftJustified());
				AnsiConsole.MarkupLineInterpolated($"\t[dim]{csFile.Value}[/]");
				var fileBasedWrapper = new FileBasedAppWrapper(csFile);
				foreach (var packageDirectiveWrapper in fileBasedWrapper.PackageDirectives)
				{
					AnsiConsole.MarkupLineInterpolated($"\t\t[blue]- {packageDirectiveWrapper.PackageInfo!.Name} {packageDirectiveWrapper.PackageInfo.Version}[/]");
				}
				
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
			RootPathAbsolute = this.TryGetDirectory(RootFolder, allowEmpty: true, shouldExist: true, PredefinedRootPath.CurrentDirectory);         
    
			return base.DoValidate();
		}
	}
}