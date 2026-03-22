# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

## [0.17.0-dev-01]

### FileBasedApp.Toolkit

- Added `FindInFiles` extension methods for stream-predicate and regex-based file content search
- Added `Replace` extension method for in-place file text transformation
- Added `FileSearchStrategy` enum (ByLine, AllText)
- Added `SimpleExecRunner.Init` static factory method
- Renamed test file from AbsolutePathExtensionsTest to IOExtensionsTest
- Added `CommandCli` namespace with `ExtendedCommandSettings`, an abstract base for Spectre.Console `CommandSettings` with convenience methods for resolving file/directory paths, loading settings from files, and reading values from environment variables
- Added `IDeserializer` interface for synchronous and asynchronous stream deserialization
- Added `JsonDeserializer` implementing `IDeserializer` using `System.Text.Json` with web-compatible defaults
- Added `FlexibleFilePath` record for cross-platform path resolution (Windows/Unix) usable in JSON configuration
- Added `LoadSetting<T>` methods on `ExtendedCommandSettings` for deserializing settings objects from files
- Fixed stream leak in `LoadSetting<T>` overloads — the deserialization stream is now properly disposed

### FileBasedApp.Toolkit.CSharp

- New project added
- Added `FileBasedAppEvaluator` — uses Roslyn to detect file-based apps by inspecting leading trivia for `#:package`, `#:property`, `#:sdk`, or `#:project` directives
- Renamed `CsharpProjectLoader` to `CsharpProjectAnalysis` and changed from abstract to concrete class
- Added `IDisposable` and `IAsyncDisposable` to `CsharpProjectAnalysis` to properly dispose `MSBuildWorkspace`
- Added fluent `Load` extension method on `CsharpProjectAnalysis` for method chaining
- Added public `Project`, `Workspace`, and `Compilation` properties with initialization guards
- Added file existence validation in `InternalLoad`

### FileBasedApp.Toolkit.Recipes

- New project added

## [0.16.0]

### FileBasedApp.Toolkit

- Added SimpleExecRunner: a fluent builder for constructing and executing system commands via SimpleExec (Run, RunAsync, ReadAsync)
- Added ISimpleExecCommandWrapper and SimpleExecCommand for testable wrapping of SimpleExec's static Command class
- Updated TruePath dependency to 1.12.0
- Decoupled direct dependency on TruePath.TestableIO.System.IO (now a package reference instead of project reference)
- Added overloads for AddArguments
- Added convenience methods
