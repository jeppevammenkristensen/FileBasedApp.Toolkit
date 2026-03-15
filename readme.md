# FileBasedApp.Toolkit

[![FileBasedApp.Toolkit](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.svg?style=flat-square&label=FileBasedApp.Toolkit)](https://www.nuget.org/packages/FileBasedApp.Toolkit)
[![TruePath.TestableIO.System.IO](https://img.shields.io/nuget/v/TruePath.TestableIO.System.IO.svg?style=flat-square&label=TruePath.TestableIO.System.IO)](https://www.nuget.org/packages/TruePath.TestableIO.System.IO)
[![FileBasedApp.Toolkit.Template](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.Template.svg?style=flat-square&label=FileBasedApp.Toolkit.Template)](https://www.nuget.org/packages/FileBasedApp.Toolkit.Template)

A toolkit for building file-based applications in .NET.

## Packages

- [FileBasedApp.Toolkit](Source/FileBasedApp/FileBasedApp.Toolkit/README.md) - Core toolkit with helpers and extensions.
- [TruePath.TestableIO.System.IO](Source/TruePath.TestableIO/TruePath.TestableIO.System.IO/README.md) - Bridges TruePath and TestableIO.
- [FileBasedApp.Toolkit.Template](Source/FileBasedApp/Templates/README.md) - Item template for creating a file based app that uses the FileBasedApp.Toolkit.

## Preview packages

Preview versions of packages are published to [GitHub Packages](https://github.com/jeppevammenkristensen/FileBasedApp.Toolkit/packages) before they are released to nuget.org. To use preview packages, add the GitHub NuGet source:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/jeppevammenkristensen/index.json" \
  --name "<NameForSource>" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Your GitHub username can be found at [github.com/settings/profile](https://github.com/settings/profile). To create a Personal Access Token (PAT) with the required `read:packages` scope, follow the [GitHub guide for creating a PAT](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic).

## Samples

- [wrapper.cs](Samples/wrapper.cs) - Generates an interface and a wrapper class for a given static or public class. It was used to generate `ISimpleExecCommandWrapper` and `SimpleExecCommand` from the SimpleExec library's static `Command` class.
