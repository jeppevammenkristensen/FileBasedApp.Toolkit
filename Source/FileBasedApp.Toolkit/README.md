# FileBasedApp.Toolkit

A collection of opinionated helpers and extensions for building file-based applications in .NET.

## Features

* Provides a `PathUtil` class for working with paths and validating string paths
* Provides an IO class to provide methods and extension methods for `TruePath`
* Provides an extension of the `CommandSettings` from the `Spectre.Console.Cli` providing validation of file and directory paths
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

## Template

You can use the `FileBasedApp.Toolkit.Template` to easily create a new filebase app with FileBasedApp.Toolkit references added https://www.nuget.org/packages/FileBasedApp.Toolkit.Template/

## Bugs or things missing

Feel free to create an issue or submit a pull request. 
