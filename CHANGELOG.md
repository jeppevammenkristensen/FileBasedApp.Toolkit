# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### FileBasedApp.Toolkit

- Added `FindInFiles` extension methods for stream-predicate and regex-based file content search
- Added `Replace` extension method for in-place file text transformation
- Added `FileSearchStrategy` enum (ByLine, AllText)
- Added `SimpleExecRunner.Init` static factory method
- Renamed test file from AbsolutePathExtensionsTest to IOExtensionsTest

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
