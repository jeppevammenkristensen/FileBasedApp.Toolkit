# FileBasedApp.Toolkit

A collection of opinionated helpers and extensions for building file-based applications in .NET.

## Features

* Provides a `PathUtil` class for working with paths and validating string paths
* Provides `LocalPathExtensions` with `IsHome`, `HasHomeDirectoryPrefix()`, and `ExpandHomeDirectory()` for home-directory expansion, `IsCurrentDirectory()` for lexical current-directory detection, plus `GetSegments()` and `Traverse()` for lexical path traversal
* Provides an IO class to provide methods and extension methods for `TruePath`
  * `FindInFiles` — filter files by stream predicate or regex (with `ByLine` / `AllText` strategies)
  * `Replace` — in-place file text transformation
  * `SafeDeleteDirectory` — safely delete directories with optional exception handling
* Provides `StringExtensions` with `StringJoin`, `IsNullOrWhitespace`, and `IsNullOrEmpty` convenience methods
* Provides an extension of the `CommandSettings` from the `Spectre.Console.Cli` providing validation of file and directory paths
* Provides a `SimpleExecRunner` fluent builder for constructing and executing system commands via the `SimpleExec` library, with support for secrets redaction and `TruePath` integration
* Includes the following libraries
  * `TruePath` 2.0.0
  * `SimpleExec`
  * `Spectre.Console.Cli`
  * `TestableIO.System.IO.Abstractions.Wrappers`
  * `TruePath.TestableIO.System.IO` 1.1.0

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

### Home-directory expansion

`HasHomeDirectoryPrefix()` detects a leading `~` segment in a relative path. `ExpandHomeDirectory()` replaces that segment with the user profile directory, preserving the remaining segments. Absolute paths and relative paths without a home-directory prefix are returned unchanged; this does not convert arbitrary relative paths to absolute paths.

```csharp
using FileBasedApp.Toolkit;
using TruePath;

var path = new LocalPath("~/Documents/file.txt");
var hasHomePrefix = path.HasHomeDirectoryPrefix(); // true
var expanded = path.ExpandHomeDirectory(); // User profile directory / Documents / file.txt
```

`IsHome` identifies only the `~` marker itself. `GetSegments()` enumerates segments root-first, while `Traverse()` enumerates them leaf-first.

### Current-directory paths and traversal

`IsCurrentDirectory()` checks the normalized path lexically: empty input, `.` and `a/..` all represent the current-directory marker. An absolute path is not a marker, even when it points to the current working directory. This check does not access the file system.

```csharp
using FileBasedApp.Toolkit;
using TruePath;

var path = new LocalPath("a/..");
var isCurrentDirectory = path.IsCurrentDirectory(); // true; Value is "." in TruePath 2.0
var segments = path.GetSegments(); // No segments; Traverse() is also empty
```

Traversal preserves absolute roots and relative parent markers (`..`). Windows rooted-without-drive and drive-relative traversal/reconstruction remain unresolved; do not assume that segment reconstruction preserves those path forms.

### TruePath 2.0 migration notes

The toolkit now references TruePath 2.0.0 and the TestableIO bridge 1.1.0. TruePath normalizes empty input and paths such as `a/..` to `.`; the toolkit still returns no traversal segments for these paths.

On Windows, `PathUtil.AnalyzeFile()` and `AnalyzeDirectory()` resolve inputs against a supplied absolute root as follows (root: `C:\base`):

| Input | Resolved path |
| --- | --- |
| `\logs` | `C:\logs` |
| `C:logs` | `C:\base\logs` |
| `C:` | `C:\base` |
| `D:logs` | Resolved against the process's current directory for drive D |
| `D:\logs` | `D:\logs` |

Drive-relative inputs are currently accepted. Cross-drive relative resolution depends on process/per-drive state, not just the supplied root; use fully qualified paths when deterministic resolution is required. Windows-only regression tests cover these cases for both file and directory analysis.

#### Absolute-path conversions require fully qualified input

`AsAbsolutePath()` (string and character-span overloads) and `AsRequiredAbsolutePath()` validate
fully qualified paths; they do not resolve relative input against the current directory.

On Windows, `C:\foo` is valid, but `\foo` (unspecified drive), `C:foo`, and `C:`
(relative to a drive's current directory) throw `ArgumentException` under TruePath 2.0.
On Unix, use an absolute path such as `/foo`. Empty and relative input also throw.

The nullable-string `AsAbsolutePath()` returns null **only for null input**, not for an invalid
path. `AsRequiredAbsolutePath()` throws `ArgumentNullException` for null. The span overload
throws for invalid or empty input rather than returning null.

Use `PathUtil.AnalyzeFile()` or `AnalyzeDirectory()` when resolution against a supplied root
is intended, or keep a relative path as a `LocalPath` instead of converting it.

#### Breaking change in the next minor release: FlexibleFilePath accepts relative paths

This behavioral breaking change will ship in a **minor release**, not a major release.
`FlexibleFilePath.GetAsLocalPath()` and its implicit conversion to `LocalPath` now construct
a `LocalPath` directly, selecting `Windows` on Windows and `Unix` elsewhere. Previously they
constructed an `AbsolutePath`, rejecting ordinary relative input.

Relative paths such as `folder/file.txt` are now accepted and remain relative; they are not
resolved against the current directory. Empty input and `.` represent the current-directory
marker. Windows inputs such as `\foo`, `C:foo`, and `C:` are also accepted without resolving
their drive or current-directory dependencies. Fully qualified paths remain supported.

**Migration:** Do not assume the returned `LocalPath` is absolute or rely on conversion throwing
for relative input. If an absolute path is required, validate it explicitly with
`AbsolutePath.Create(localPath.Value)` (which rejects non-fully-qualified input), or use
`PathUtil.AnalyzeFile()` / `AnalyzeDirectory()` to resolve it against a supplied root.

```csharp
var configured = new FlexibleFilePath(@"data\file.txt", "data/file.txt");
LocalPath path = configured; // Relative on either platform; no longer throws
```

### SimpleExecRunner

The `SimpleExecRunner` provides a fluent builder API on top of the [SimpleExec](https://github.com/adamralph/simple-exec) library for constructing and executing system commands. It supports `TruePath` types (`AbsolutePath`, `LocalPath`) directly as arguments, and can redact secrets from echoed output.

```csharp
using FileBasedApp.Toolkit.SimpleExec;
using Spectre.Console;
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

AnsiConsole.WriteLine(result.ExitCode.ToString());
AnsiConsole.WriteLine(result.StandardOutput);
AnsiConsole.WriteLine(result.StandardError);
```

`RunWithExitCode()` and `RunWithExitCodeAsync()` return `SimpleExecRunResult`, which contains `ExitCode`. `ReadEnhancedAsync()` returns `SimpleExecReadResult`, which contains `ExitCode`, `StandardOutput`, and `StandardError`. These methods still honor the configured exit-code policy.

**Key types:**
* `SimpleExecRunner` — the fluent builder. Create with `new SimpleExecRunner("command-name")`, add arguments, then call `Run()`, `RunAsync()`, `ReadAsync()`, or a result-returning execution method.
* `SimpleExecRunResult` / `SimpleExecReadResult` — result types returned by the exit-code-aware execution methods.
* `ISimpleExecCommandWrapper` — an interface wrapping `SimpleExec.Command` to enable unit testing. The default implementation (`SimpleExecCommand`) delegates directly to the static `Command` class.

### Web URIs (AbsoluteWebUri / RelativeWebUri)

Strongly-typed, immutable wrappers around `System.Uri` for composing web URIs. Path segments, query strings, and fragments are represented as validated value objects (`UriPathSegment`, `UriQueryString`, `UriFragment`) and can be combined fluently via the `/` operator.

```csharp
using FileBasedApp.Toolkit;
using Spectre.Console;

// Build an absolute URI fluently
var url = AbsoluteWebUri.Create("https://example.com")
          / UriPathSegment.From("first")
          / UriPathSegment.From("second")
          / UriQueryString.From("a=1&b=2")
          / UriFragment.From("Fragment");

// https://example.com/first/second?a=1&b=2#Fragment
AnsiConsole.WriteLine(url.ToString());

// Or use method form
var api = AbsoluteWebUri.Create("https://example.com/api")
    .AddPathSegment(UriPathSegment.From("users"))
    .AddQueryPart("page", "1")
    .WithFragment(UriFragment.From("top"));

// Replace the entire path with a single segment (preserves query/fragment)
var reset = AbsoluteWebUri.Create("https://example.com/old/path?q=1")
    .WithPathSegment(UriPathSegment.From("new"));
// https://example.com/new?q=1

// Relative URIs work the same way
var relative = RelativeWebUri.Create("/search")
               / UriQueryString.From("q=hello");
```

Each operation returns a new instance, so the originals are never mutated. Invalid inputs (empty segments, illegal characters) throw at the value-object construction step via Vogen validation.

### HTTP extensions

`AbsoluteWebUriHttpExtensions` adds a `WithBaseAddress` extension on `HttpClient` that accepts an `IWebUri<TSelf>`, and `HttpResponseMessageExtensions` adds `ToJson<T>` / `ToRequiredJson<T>` for deserializing response content via a source-generated `JsonTypeInfo<T>`. Both deserialization helpers call `EnsureSuccessStatusCode` first; `ToRequiredJson` additionally throws `InvalidOperationException` when deserialization yields `null`.

```csharp
using System.Text.Json.Serialization;
using FileBasedApp.Toolkit;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Widget))]
internal partial class AppJsonContext : JsonSerializerContext { }

public record Widget(string Name, int Count);

var baseUri = AbsoluteWebUri.Create("https://example.com/api");
var httpClient = new HttpClient().WithBaseAddress(baseUri);

using var response = await httpClient.GetAsync("widgets?id=1");
var widget = await response.ToRequiredJson(AppJsonContext.Default.Widget);
```

## Template

You can use the `FileBasedApp.Toolkit.Template` to easily create a new filebase app with FileBasedApp.Toolkit references added https://www.nuget.org/packages/FileBasedApp.Toolkit.Template/

## Bugs or things missing

Feel free to create an issue or submit a pull request. 

## Credits

[Floppy disc icons created by IYAHICON - Flaticon](https://www.flaticon.com/free-icons/floppy-disc "floppy disc icons")
