# FileBasedApp.Toolkit.Spectre

Optional Spectre.Console and Spectre.Console.Cli integration.

- `FileBasedApp.Toolkit.AnsiConsoleFactory` creates stderr and quiet consoles.
- `FileBasedApp.Toolkit.CommandCli.ExtendedCommandSettings` adds exception-based
  validation, path resolution, environment fallback and settings-file loading.

This package references FileBasedApp.Toolkit, Spectre.Console and Spectre.Console.Cli,
but does not depend on SimpleExec. Deserializers and FlexibleFilePath remain in core.

## Migration (1.4.0-preview-01)

These types moved from FileBasedApp.Toolkit into this optional package. Add an explicit reference:

```xml
<PackageReference Include="FileBasedApp.Toolkit.Spectre" Version="1.4.0-preview-01" />
```

Namespaces remain unchanged, but the assembly move is a **binary breaking change**.
Recompile existing consumers after adding the reference. The core package remains
framework-neutral; it no longer brings in Spectre.Console or Spectre.Console.Cli.

## Extended settings

The example below shows how to use the `ExtendedCommandSettings` class to validate file and directory paths.

```csharp
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.CommandCli;
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
