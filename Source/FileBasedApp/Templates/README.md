# File-Based App Templates

This package contains `dotnet new` templates for building file-based applications using the [FileBasedApp.Toolkit](https://www.nuget.org/packages/FileBasedApp.Toolkit).

## Installation

Install the templates from NuGet:

```bash
dotnet new install FileBasedApp.Toolkit.Template
```

## Usage

After installation, you can create a new file-based application project or class using:

```bash
dotnet new filebasedtoolkitapp -n MyFileBasedApp
```

The generated project will include the basic structure and boilerplate required to build an application that operates on files and directories, leveraging the helpers and utilities provided by the `FileBasedApp.Toolkit`.

The generated file references `FileBasedApp.Toolkit.Spectre` for its Spectre.Console command and settings, and `FileBasedApp.Toolkit.SimpleExec` for running external commands. Both integration packages depend on the core toolkit; their types retain their existing namespaces.

## Learn More

- [FileBasedApp.Toolkit GitHub Repository](https://github.com/jeppevammenkristensen/FileBasedApp.Toolkit)
- [Main Toolkit NuGet Package](https://www.nuget.org/packages/FileBasedApp.Toolkit)

## Credits

[Floppy disc icons created by IYAHICON - Flaticon](https://www.flaticon.com/free-icons/floppy-disc "floppy disc icons")
