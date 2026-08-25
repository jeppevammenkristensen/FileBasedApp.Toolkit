# FileBasedApp.Toolkit

[![FileBasedApp.Toolkit](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.svg?style=flat-square&label=FileBasedApp.Toolkit)](https://www.nuget.org/packages/FileBasedApp.Toolkit)
[![FileBasedApp.Toolkit.CSharp](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.CSharp.svg?style=flat-square&label=FileBasedApp.Toolkit.CSharp)](https://www.nuget.org/packages/FileBasedApp.Toolkit.CSharp)
[![FileBasedApp.Toolkit.Dotnet](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.Dotnet.svg?style=flat-square&label=FileBasedApp.Toolkit.Dotnet)](https://www.nuget.org/packages/FileBasedApp.Toolkit.Dotnet)
[![FileBasedApp.Toolkit.Template](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.Template.svg?style=flat-square&label=FileBasedApp.Toolkit.Template)](https://www.nuget.org/packages/FileBasedApp.Toolkit.Template)

A toolkit for building file-based applications in .NET.  (see  [more](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps))

**NOTE** It was made for FileBased Apps. But the functionality can be used in standard console apps or in libaries. 

## Packages

- [FileBasedApp.Toolkit](Source/FileBasedApp/FileBasedApp.Toolkit/README.md) - Core toolkit with helpers and extensions.
- [TruePath.TestableIO.System.IO](https://github.com/jeppevammenkristensen/TruePath.TestableIO.System.IO) - External bridge between TruePath and TestableIO.
- [FileBasedApp.Toolkit.CSharp](Source/FileBasedApp/FileBasedApp.Toolkit.CSharp/README.md) - Roslyn-based helpers for detecting and working with file-based .NET apps.
- [FileBasedApp.Toolkit.Dotnet](Source/FileBasedApp/FileBasedApp.Toolkit.Dotnet/README.md) - Fluent builders and recipes for dotnet CLI commands (pack, nuget push, package search).
- [FileBasedApp.Toolkit.Template](Source/FileBasedApp/Templates/README.md) - Item template for creating a file based app that uses the FileBasedApp.Toolkit.

It takes some inspiration based on experiences with brilliant tools like Cake and Nuke. But tries to give some of what those libraries provides but aimed at just using FileBasedApps.

It relies heavily (hence opinionated) on [TruePath](https://github.com/ForNeVeR/TruePath),[SimpleExec](https://github.com/adamralph/simple-exec) and [Spectre.Console and Spectre.Console.Cli](https://spectreconsole.net/). In this library I strive for especially TruePath types to be first class citizens.

`TruePath.TestableIO.System.IO` is maintained in its [own repository](https://github.com/jeppevammenkristensen/TruePath.TestableIO.System.IO) and provides a bridge between `TruePath` and [System.Io.Abstractions](https://github.com/TestableIO/System.IO.Abstractions).

## Scripts

This project uses FileBasedApp.Toolkit itself to power its own CI/CD pipeline. The [Scripts](Scripts/) folder contains `.cs` scripts that reference `FileBasedApp.Toolkit` as a `#:package` dependency and are executed directly with `dotnet run`:

- [build-and-run-test.cs](Scripts/build-and-run-test.cs) - Builds the solution and runs tests, used by the [Build and run Tests](.github/workflows/dotnet.yml) workflow on every push and pull request. Uses `FileBasedApp.Toolkit`.
- [build-and-publish-nuget-packages.cs](Scripts/build-and-publish-nuget-packages.cs) - Packs and publishes NuGet packages to either GitHub Packages (staging) or nuget.org (production), used by the [Publish nuget packages](.github/workflows/publish.yml) workflow. Uses `FileBasedApp.Toolkit`.
- [bump-version.cs](Scripts/bump-version.cs) - Bumps the version in `Source/FileBasedApp/Directory.Build.props`, supporting alpha/rc pre-release increments. Uses `FileBasedApp.Toolkit`.
- [bump-publish-and-update.cs](Scripts/bump-publish-and-update.cs) - Interactive orchestrator that chains `bump-version.cs`, `build-and-publish.cs`, and `update-filebased-package-references.cs` based on user selection. Uses `FileBasedApp.Toolkit`.
- [list-filebased-apps.cs](Scripts/list-filebased-apps.cs) - Locates file-based apps by scanning for `#:` directives with Roslyn and regex, then lists or copies their paths. Uses `FileBasedApp.Toolkit`.
- [update-filebased-package-references.cs](Scripts/update-filebased-package-references.cs) - Finds file-based apps in the repo and updates their `#:package` references to the latest `FileBasedApp.Toolkit` package versions. Uses `FileBasedApp.Toolkit.CSharp` and `FileBasedApp.Toolkit.Dotnet`.

## Samples

The scripts in the [Samples](Samples/) folder are file-based .NET apps that reference the `FileBasedApp.Toolkit` packages via `#:package` directives and demonstrate how to use them.

- [wrapper.cs](Samples/wrapper.cs) - Generates an interface and a wrapper class for a given static or public class. It was used to generate `ISimpleExecCommandWrapper` and `SimpleExecCommand` from the SimpleExec library's static `Command` class. Uses `FileBasedApp.Toolkit`.
- [build-and-publish.cs](Samples/build-and-publish.cs) - Builds, packs, and publishes NuGet packages for file-based app projects. Uses `FileBasedApp.Toolkit`, `FileBasedApp.Toolkit.CSharp`, and `FileBasedApp.Toolkit.Dotnet`.
- [filebased-app-finder.cs](Samples/filebased-app-finder.cs) - Discovers file-based .NET apps in a directory and retrieves the latest package versions for their dependencies. Uses `FileBasedApp.Toolkit.CSharp` and `FileBasedApp.Toolkit.Dotnet`.
- [website-opener.cs](Samples/website-opener.cs) - Fetches JSON from a web endpoint using `AbsoluteWebUri` and deserializes the response with a source-generated `JsonSerializerContext`. Uses `FileBasedApp.Toolkit`.

## Preview packages

To not flood nuget.org with packages alpha version of the toolkit and inital release candidates are added as preview packages in github.

Preview versions of packages are published to [GitHub Packages](https://github.com/jeppevammenkristensen/FileBasedApp.Toolkit/packages) before they are released to nuget.org. To use preview packages, add the GitHub NuGet source:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/jeppevammenkristensen/index.json" \
  --name "<NameForSource>" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Your GitHub username can be found at [github.com/settings/profile](https://github.com/settings/profile). To create a Personal Access Token (PAT) with the required `read:packages` scope, follow the [GitHub guide for creating a PAT](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic).
