#:package FileBasedApp.Toolkit.CSharp@0.18.0-alpha-11
#:package FileBasedApp.Toolkit.Dotnet@0.18.0-alpha-09
#:package Dumpify@*

#:property PublishAot=false

using System.ComponentModel;
using Spectre.Console.Cli;
using TruePath;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SimpleExec;
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
	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{	
		AnsiConsole.MarkupLineInterpolated($"[green]RootPath is {settings.RootPathAbsolute.Value}[/]");
		
		Dictionary<string, string> NameVersion = new Dictionary<string, string>();
		var result =await DotnetRecipes.GetPackageInformation("FileBasedApp.Toolkit", true);

		void AddVersion(string name)
		{
			var highestVersion = result.GetHighestVersion(name);
			AnsiConsole.MarkupLineInterpolated($"[blue]Adding version for {name} {highestVersion.LatestVersion}[/]");
			NameVersion.Add(name, highestVersion.LatestVersion);	
		}
		
		AddVersion("FileBasedApp.Toolkit");
		AddVersion("FileBasedApp.Toolkit.CSharp");
		AddVersion("FileBasedApp.Toolkit.Dotnet");
		
		foreach (var csFile in settings.RootPathAbsolute.EnumerateAllFiles("*.cs"))
		{	
			if (await csFile.IsFileBasedAppAsync(token:cancellationToken))
			{			
				AnsiConsole.Write(new Rule(csFile.FileName).LeftJustified());
				AnsiConsole.MarkupLineInterpolated($"{csFile.FileName}");
				
				var fileBasedWrapper = new FileBasedAppWrapper(csFile);
				foreach (var item in fileBasedWrapper.PackageDirectives.Where(x => NameVersion.ContainsKey(x.PackageInfo?.Name ?? string.Empty)))
				{
					var version = NameVersion[item.PackageInfo.Name];					
					
					if (item.PackageInfo!.Version.Value != version)
					{
						item.PackageInfo.Version.Value = version;
						var compilationUnitSyntax = item.Update(fileBasedWrapper.CompilationUnitSyntax);								
						fileBasedWrapper.CompilationUnitSyntax = compilationUnitSyntax;
						fileBasedWrapper.Save();
						AnsiConsole.MarkupLineInterpolated($"[green]Saved: {item.PackageInfo.Version}[/]");
					}
					
				}
				
				if (PathUtil.GetExecutionFile().Equals(csFile))
				{
					AnsiConsole.MarkupLineInterpolated($"[yellow]Skipping build of {csFile.FileName} since it's the execution file[/]");
					continue;
				}
				
				await SimpleExecRunner.Init("dotnet")
					.AddArgumentPair("build", csFile)
					.AddArgument("--no-cache")
					.RunAsync();
				
				//fileBasedWrapper.Path.Value.DumpConsole();
			}
		}
		
		return 0; // 0 for success
	}

	public class Settings : ExtendedCommandSettings
	{
		[CommandArgument(0, "[RootFolder]")]
		[DefaultValue("..")]
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