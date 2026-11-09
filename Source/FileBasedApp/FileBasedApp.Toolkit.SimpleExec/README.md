# FileBasedApp.Toolkit.SimpleExec

Optional SimpleExec integration: fluent command runners, typed path arguments,
exit-code handling, JSON output parsing and testable command wrappers.

Use the existing `FileBasedApp.Toolkit.SimpleExec` namespace. This package references
FileBasedApp.Toolkit and SimpleExec, but does not depend on Spectre.

## Migration (1.4.0-preview-01)

These types moved from FileBasedApp.Toolkit into this optional package. Add an explicit reference:

```xml
<PackageReference Include="FileBasedApp.Toolkit.SimpleExec" Version="1.4.0-preview-01" />
```

Namespaces remain unchanged, but the assembly move is a **binary breaking change**.
Recompile existing consumers after adding the reference. The core package remains
framework-neutral; it no longer brings in SimpleExec.

## SimpleExecRunner

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

// Working directory and environment variables
await new SimpleExecRunner("git")
    .AddArguments("status", "--porcelain")
    .WithWorkingDirectory(repoRoot)
    .WithConfigureEnvironment(env => env["GIT_TERMINAL_PROMPT"] = "0")
    .RunAsync();

// Accept an expected non-zero exit code. `git diff --quiet` returns 1 when
// differences exist, which is an expected result rather than a command failure.
await new SimpleExecRunner("git")
    .AddArguments("diff", "--quiet")
    .WithAcceptedErrorCodes([1])
    .RunAsync();
```

Exit code `0` remains successful by default and does not need to be listed. Other non-zero exit codes still cause the command to throw. Use `WithExitCodeHandler` when you need the lower-level handled-exit-code callback instead of a fixed list of accepted codes.

Use `HandlesAllErrorCodes()` only when every exit code is an expected result; it disables SimpleExec's default non-zero-exit-code failure behavior.

Use the result-returning APIs when the observed exit code or captured output is needed after execution:

```csharp
var result = await new SimpleExecRunner("git")
    .AddArguments("diff", "--quiet")
    .WithAcceptedErrorCodes([1])
    .ReadEnhancedAsync();

Console.WriteLine(result.ExitCode);
Console.WriteLine(result.StandardOutput);
Console.WriteLine(result.StandardError);
```

`RunWithExitCode()` and `RunWithExitCodeAsync()` return `SimpleExecRunResult`, which contains `ExitCode`. `ReadEnhancedAsync()` returns `SimpleExecReadResult`, which contains `ExitCode`, `StandardOutput`, and `StandardError`. These methods still honor the configured exit-code policy.

**Key types:**
* `SimpleExecRunner` — the fluent builder. Create with `new SimpleExecRunner("command-name")`, add arguments, then call `Run()`, `RunAsync()`, `ReadAsync()`, or a result-returning execution method.
* `SimpleExecRunResult` / `SimpleExecReadResult` — result types returned by the exit-code-aware execution methods.
* `ISimpleExecCommandWrapper` — an interface wrapping `SimpleExec.Command` to enable unit testing. The default implementation (`SimpleExecCommand`) delegates directly to the static `Command` class.
