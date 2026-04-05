# FileBasedApp.Toolkit

A collection of opinionated helpers and extensions for building file-based applications in .NET.

## Features

* Provides a `PathUtil` class for working with paths and validating string paths
* Provides an IO class to provide methods and extension methods for `TruePath`
  * `FindInFiles` — filter files by stream predicate or regex (with `ByLine` / `AllText` strategies)
  * `Replace` — in-place file text transformation
  * `SafeDeleteDirectory` — safely delete directories with optional exception handling
* Provides `StringExtensions` with `StringJoin`, `IsNullOrWhitespace`, and `IsNullOrEmpty` convenience methods
* Provides an extension of the `CommandSettings` from the `Spectre.Console.Cli` providing validation of file and directory paths
* Provides a `SimpleExecRunner` fluent builder for constructing and executing system commands via the `SimpleExec` library, with support for secrets redaction and `TruePath` integration
* Includes the following libraries
  * `TruePath`
  * `SimpleExec`
  * `Spectre.Console.Cli`
  * `TestableIO.System.IO.Abstractions.Wrappers`
  * `TruePath.TestableIO.System.IO`

## Example

### Extended settings

The example below shows how to use the `ExtendedCommandSettings` class to validate file and directory paths.

```csharp
using FileBasedApp.Toolkit;
using Spectre.Console;
using Spectre.Console.Cli;
using TruePath;

public class CustomCommandSettings : ExtendedCommandSettings
{
    [CommandArgument(0, "<directory-path>")]
    public string? Directory { get; set; }
    
    [CommandOption("--filePath")]
    public required string File { get; set; }

    public AbsolutePath DirectoryPath { get; set; }
    
    public AbsolutePath FilePath { get; set; }
    
    protected override ValidationResult DoValidate()
    {
        // Evaluates the directory string.
        DirectoryPath = TryGetDirectory(Directory,
            allowEmpty: true,
            shouldExist: true,
            PredefinedRootPath.ExecutionFolder);
        FilePath = TryGetFile(File, true, roots: [PathUtil.GetCurrentWorkingFolder(), PathUtil.GetExecutionFolder()]);
        return base.DoValidate();
    }
}
```

### TruePath IO extensions

```csharp
using System.IO.Abstractions;
using FileBasedApp.Toolkit;

var applicationData = Environment.SpecialFolder.ApplicationData.GetSpecialFolder();
applicationData.FindRequiredParent(x => x.FileName == "SomeValue");
var ancestors = applicationData.GetAncestors(true).ToList();

// Combining with TruePath.TestableIO.System.IO
// This was orignally part of the FileBased.Toolkit library but moved
// to it's own library
var newDirectory = applicationData / "NewDirectory";
newDirectory.CreateDirectory();

IFileSystem fileSystem = new FileSystem();
fileSystem.File.Create(newDirectory / "test.txt");

(newDirectory / "..").GetDirectories(fileSystem)
```

### SimpleExecRunner

The `SimpleExecRunner` provides a fluent builder API on top of the [SimpleExec](https://github.com/adamralph/simple-exec) library for constructing and executing system commands. It supports `TruePath` types (`AbsolutePath`, `LocalPath`) directly as arguments, and can redact secrets from echoed output.

```csharp
using FileBasedApp.Toolkit.SimpleExec;
using TruePath;

// Basic command execution
await new SimpleExecRunner("dotnet")
    .AddArgument("build")
    .AddArgument("MyProject.csproj")
    .RunAsync();

// Using AddArgumentPair for flag + value combinations
await new SimpleExecRunner("dotnet")
    .AddArgument("pack")
    .AddArgumentPair("-c", "Release")
    .AddArgumentPair("-o", outputPath) // accepts AbsolutePath directly
    .RunAsync();

// With secrets redaction — the API key value is replaced with "***" in echoed output
await new SimpleExecRunner("dotnet")
    .AddArguments("nuget", "push")
    .AddArgument(packagePath)
    .AddArgumentPair("--source", "nuget.org")
    .AddArgumentPair("--api-key", apiKey, isSecret: true)
    .RunAsync();

// Reading command output
var (stdout, stderr) = await new SimpleExecRunner("dotnet")
    .AddArgument("--version")
    .ReadAsync();

// Working directory, environment variables, and custom exit code handling
await new SimpleExecRunner("git")
    .AddArguments("status", "--porcelain")
    .WithWorkingDirectory(repoRoot)
    .WithConfigureEnvironment(env => env["GIT_TERMINAL_PROMPT"] = "0")
    .WithExitCodeHandler(code => code is 0 or 1)
    .RunAsync();
```

**Key types:**
* `SimpleExecRunner` — the fluent builder. Create with `new SimpleExecRunner("command-name")`, add arguments, then call `Run()`, `RunAsync()`, or `ReadAsync()`.
* `ISimpleExecCommandWrapper` — an interface wrapping `SimpleExec.Command` to enable unit testing. The default implementation (`SimpleExecCommand`) delegates directly to the static `Command` class.

## Template

You can use the `FileBasedApp.Toolkit.Template` to easily create a new filebase app with FileBasedApp.Toolkit references added https://www.nuget.org/packages/FileBasedApp.Toolkit.Template/

## Bugs or things missing

Feel free to create an issue or submit a pull request. 

## Credits

[Floppy disc icons created by IYAHICON - Flaticon](https://www.flaticon.com/free-icons/floppy-disc "floppy disc icons")
