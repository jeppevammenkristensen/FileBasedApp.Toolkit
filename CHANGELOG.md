# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## FileBasedApp

### [0.18.0]

#### FileBasedApp.Toolkit.Dotnet

- New project added
- Added `DotnetBaseRunner<TSelf>` — abstract base class for fluent `dotnet` CLI command runners with `--configuration` and `--verbosity` options
- Added `DotnetPackSimpleRunner` — fluent wrapper around `dotnet pack` with options for output, symbols, versioning, runtime, and more
- Added `DotnetNugetPushSimpleRunner` — fluent wrapper around `dotnet nuget push` with options for source, API key, skip-duplicate, timeout, and more
- Added `DotnetRecipes` — higher-level helpers composing dotnet CLI commands (e.g. `GetPackageInformation` for JSON-based NuGet package search)
- Added `PackageRequest`, `SearchResult`, and `Package` records for deserializing `dotnet package search --format json` output
- Added `PackageRequestExtension` with `FlattenedPackages` and `GetHighestVersion` helpers

#### FileBasedApp.Toolkit

- Extracted `BaseSimpleExecRunner<TSelf>` from `SimpleExecRunner` to enable reusable fluent command runner base classes

#### FileBasedApp.Toolkit.CSharp

- Breaking changes compared to 0.17.0
- Added CsharpSolutionAnalysis
- Fix inconsistent naming. Csharp has been renamed CSharp

### [0.17.0]

#### FileBasedApp.Toolkit

- Added `FindInFiles` extension methods for stream-predicate and regex-based file content search
- Added `Replace` extension method for in-place file text transformation
- Added `FileSearchStrategy` enum (ByLine, AllText)
- Added `SafeDeleteDirectory` extension method for safely deleting directories with optional exception handling
- Added `SimpleExecRunner.Init` static factory method
- Made `StringExtensions` public (was internal) and added `StringJoin<T>`, `IsNullOrWhitespace`, and `IsNullOrEmpty` convenience methods
- Renamed test file from AbsolutePathExtensionsTest to IOExtensionsTest
- Added `CommandCli` namespace with `ExtendedCommandSettings`, an abstract base for Spectre.Console `CommandSettings` with convenience methods for resolving file/directory paths, loading settings from files, and reading values from environment variables
- Added `IDeserializer` interface for synchronous and asynchronous stream deserialization
- Added `JsonDeserializer` implementing `IDeserializer` using `System.Text.Json` with web-compatible defaults
- Added `FlexibleFilePath` record for cross-platform path resolution (Windows/Unix) usable in JSON configuration
- Added `LoadSetting<T>` methods on `ExtendedCommandSettings` for deserializing settings objects from files
- Fixed stream leak in `LoadSetting<T>` overloads — the deserialization stream is now properly disposed

#### FileBasedApp.Toolkit.CSharp

- New project added
- Added `FileBasedAppEvaluator` — uses Roslyn to detect file-based apps by inspecting leading trivia for `#:package`, `#:property`, `#:sdk`, or `#:project` directives
- Added `FileEvaluationExtensions` for batch-evaluating files and directories for file-based app detection
- Renamed `CsharpProjectLoader` to `CsharpProjectAnalysis` and changed from abstract to concrete class
- Added `IDisposable` and `IAsyncDisposable` to `CsharpProjectAnalysis` to properly dispose `MSBuildWorkspace`
- Added fluent `Load` extension method on `CsharpProjectAnalysis` for method chaining
- Added public `Project`, `Workspace`, and `Compilation` properties with initialization guards
- Added file existence validation in `InternalLoad`
- Added `CompilationExtensions` with `FindImplementationOfInterface` methods and `GetNamedTypeSymbols` visitor
- Wrapped `Compilation` in `CompilationWrapper` with named type symbol caching
- Added `ResultSymbolVisitor` for visiting and collecting symbol results from compilations
- Added `RoslynExtensions` with `IsStringLike`, `IsTaskLike`, and `TryGetEnumerableElementType` type classification helpers
- Added `StringInfo` and `EnumerableInfo` records for describing string-like and enumerable type symbols

### [0.16.0]

#### FileBasedApp.Toolkit

- Added SimpleExecRunner: a fluent builder for constructing and executing system commands via SimpleExec (Run, RunAsync, ReadAsync)
- Added ISimpleExecCommandWrapper and SimpleExecCommand for testable wrapping of SimpleExec's static Command class
- Updated TruePath dependency to 1.12.0
- Decoupled direct dependency on TruePath.TestableIO.System.IO (now a package reference instead of project reference)
- Added overloads for AddArguments
- Added convenience methods

## TruePath.TestableIO.System.IO

### [0.16.0]

- Added `FileMove`, `FileCopy`, and `FileReplace` extension methods on `AbsolutePath` for file move, copy, and replace operations via `IFileSystem`
- Added `Move`, `Copy` extension methods on `IFile` for `AbsolutePath`-based file operations
- Added `DirectoryMove` extension method on `AbsolutePath` for directory move operations via `IFileSystem`
- Added `Move` extension method on `IDirectory` for `AbsolutePath`-based directory move
- Added `MoveTo` extension method on `IDirectoryInfo` for `AbsolutePath`-based directory move
