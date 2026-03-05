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

## Features

- **Pre-configured Structure**: Includes necessary project settings and initial code.
- **Toolkit Integration**: Seamlessly uses `FileBasedApp.Toolkit` for path manipulation and IO operations.
- **Modern .NET**: Targeted for the latest .NET versions.

## Learn More

- [FileBasedApp.Toolkit GitHub Repository](https://github.com/JeppeRoi/FileBasedApp.Toolkit)
- [Main Toolkit NuGet Package](https://www.nuget.org/packages/FileBasedApp.Toolkit)
