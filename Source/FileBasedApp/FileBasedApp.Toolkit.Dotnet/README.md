# FilebasedApp.Toolkit.Dotnet

Fluent builders and recipes for common `dotnet` CLI commands.

## Features

* `DotnetBaseRunner<TSelf>` — abstract base class providing shared `--configuration` and `--verbosity` options
* `DotnetPackSimpleRunner` — fluent wrapper around `dotnet pack` with options for output directory, symbols, versioning, runtime, and more
* `DotnetNugetPushSimpleRunner` — fluent wrapper around `dotnet nuget push` with options for source, API key, skip-duplicate, timeout, and more
* `DotnetRecipes` — higher-level helpers that compose dotnet CLI commands into common workflows
* `PackageRequest` / `SearchResult` / `Package` — records for deserializing `dotnet package search --format json` output
* `PackageRequestExtension` — helpers for flattening and querying package search results

## Examples

### Packing a project

```csharp
using FilebasedApp.Toolkit.Dotnet;
using TruePath;

var output = AbsolutePath.Create("/artifacts");

await DotnetPackSimpleRunner.Init()
    .WithProject(AbsolutePath.Create("/src/MyProject.csproj"))
    .WithConfiguration("Release")
    .WithOutput(output)
    .WithVersionSuffix("beta-01")
    .RunAsync();
```

### Pushing a NuGet package

```csharp
using FilebasedApp.Toolkit.Dotnet;
using TruePath;

await DotnetNugetPushSimpleRunner.Init()
    .WithPackage(AbsolutePath.Create("/artifacts/MyPackage.1.0.0.nupkg"))
    .WithSource("https://api.nuget.org/v3/index.json")
    .WithApiKey(apiKey)
    .WithSkipDuplicate()
    .RunAsync();
```

### Searching for a NuGet package

```csharp
using FilebasedApp.Toolkit.Dotnet;

var result = await DotnetRecipes.GetPackageInformation("Newtonsoft.Json", includePrerelease: false);

// Get the highest version of a specific package
var package = result.GetHighestVersion("Newtonsoft.Json");
Console.WriteLine($"{package.Id} {package.LatestVersion}");

// Flatten all packages across all sources
foreach (var (sourceName, pkg) in result.FlattenedPackages)
{
    Console.WriteLine($"[{sourceName}] {pkg.Id} {pkg.LatestVersion}");
}
```

## Bugs or things missing

Feel free to create an issue or submit a pull request.

## Credits

[Floppy disc icons created by IYAHICON - Flaticon](https://www.flaticon.com/free-icons/floppy-disc "floppy disc icons")
