# File-Based App Samples

These standalone C# files demonstrate .NET file-based apps using `FileBasedApp.Toolkit`. They restore dependencies from their `#:package` directives and run without a project file:

```powershell
dotnet run <script.cs> -- [arguments]
```

Use a .NET SDK that supports file-based apps. Run a command with `--help` to see its available arguments.

Samples using Spectre.Console commands reference `FileBasedApp.Toolkit.Spectre`; those using external command runners also reference `FileBasedApp.Toolkit.SimpleExec`. These optional integrations depend on the core toolkit and preserve the existing namespaces. The HTTP-only `website-opener.cs` sample needs neither integration.

Keep all toolkit package directives in each sample aligned to the same release version. Configure a feed containing the referenced versions before running them; see the root README's [migration and bootstrap instructions](../readme.md#integration-package-migration).

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

## Git Worktree Samples

Both apps require Git and a .NET 11 SDK that supports C# 15 unions. Keep [`Shared.cs`](worktree-path-selector/Shared.cs) beside the two entry points: both include it for worktree discovery and commit metadata. Each app accepts an optional starting directory (default: the current directory). It checks that directory first, then its immediate subdirectories, and uses the first repository found.

### Worktree Path Selector

[`worktree-path-selector.cs`](worktree-path-selector/worktree-path-selector.cs) opens a searchable picker with each worktree's branch, path, HEAD commit, and latest commit date when available. It lists the primary worktree first, then sorts the others by newest commit first. Selecting a worktree copies its path to the clipboard.

From the `Samples` directory:

```powershell
dotnet run ./worktree-path-selector/worktree-path-selector.cs -- /path/to/repository
```

See the [installation instructions](worktree-path-selector/instructions.md) for clipboard requirements and how to install the selector as a global .NET tool and configure `Push-Worktree` in PowerShell 7 on Windows, Linux, or macOS. The function enters the selected directory; `Pop-Location` returns to the previous one.

### Worktree Cleanup

[`worktree-cleanup.cs`](worktree-path-selector/worktree-cleanup.cs) lists linked worktrees from oldest to newest by latest commit date and leaves the primary worktree out of the list. Select one or more worktrees and confirm to recursively delete their directories. The script then runs `git worktree prune` from the primary worktree.

From the `Samples` directory:

```powershell
dotnet run ./worktree-path-selector/worktree-cleanup.cs -- /path/to/repository
```

**Check for uncommitted and untracked files before confirming.** Cleanup deletes directories directly, bypassing the dirty-worktree checks in `git worktree remove`. The selector installer does not install the cleanup app.
