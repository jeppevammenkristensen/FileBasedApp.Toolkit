# File-Based App Samples

These standalone C# files demonstrate .NET file-based apps using `FileBasedApp.Toolkit`. They restore dependencies from their `#:package` directives and run without a project file:

```powershell
dotnet run <script.cs> -- [arguments]
```

Use a .NET SDK that supports file-based apps. Run a command with `--help` to see its available arguments.

## Scripts

### `build-and-publish.cs`

Packs a project, solution, file-based app, or the buildable files in a directory, then pushes the resulting NuGet packages to a configured source. It prompts for an enabled NuGet source unless `--source` is supplied and supports `--api-key` and repeatable `--property NAME=VALUE` options.

```powershell
dotnet run build-and-publish.cs -- ./MyApp.cs --source LocalPackages
```

This script publishes packages. Check the selected source before confirming or supplying credentials.

### `filebased-app-finder.cs`

Recursively scans a directory for C# file-based apps and displays each app's package directives. The root directory is optional and defaults to the current directory.

```powershell
dotnet run filebased-app-finder.cs -- .
```

### `website-opener.cs`

A minimal HTTP and source-generated JSON serialization example. It requests a sample todo list from `jsonplaceholder.typicode.com` and deserializes the response.

```powershell
dotnet run website-opener.cs
```

### `wrapper.cs`

Reads C# source from the clipboard, selects a class when necessary, and generates an interface and forwarding wrapper for its public methods. It prints the result and can copy it back to the clipboard. Use `--className` and `--interfaceName` to override generated names.

```powershell
dotnet run wrapper.cs -- --className ServiceAdapter --interfaceName IServiceAdapter
```

All wrapped methods in the selected class must be either static or instance methods; mixed classes are rejected.

## Worktree Path Selector

The nested [`worktree-path-selector`](worktree-path-selector/) sample provides an interactive Git worktree picker. The C# app finds a repository, lists its worktrees, and copies the selected path to the clipboard. `push-worktree.ps1` adds a PowerShell `Push-Worktree` function that runs the picker and changes to the selected directory while preserving the previous location for `Pop-Location`.

See the [installation instructions](worktree-path-selector/instructions.md) to install the app as a global .NET tool and configure PowerShell on Windows, Linux, or macOS. This is aimed to be picked up by AI.
