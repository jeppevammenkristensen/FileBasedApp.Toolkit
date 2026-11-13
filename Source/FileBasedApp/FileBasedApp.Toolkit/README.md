# FileBasedApp.Toolkit

A collection of opinionated helpers and extensions for building file-based applications in .NET.

The core package remains framework-neutral: Spectre.Console and SimpleExec integrations
are available as separate, optional packages.

## Package split and migration (1.4.0)

If you previously used integration types from `FileBasedApp.Toolkit`, add an explicit
reference to the corresponding package:

```xml
<PackageReference Include="FileBasedApp.Toolkit.Spectre" Version="1.4.0" />
<PackageReference Include="FileBasedApp.Toolkit.SimpleExec" Version="1.4.0" />
```

Add only the integrations you use. See [Spectre integration](../FileBasedApp.Toolkit.Spectre/README.md)
for `AnsiConsoleFactory` and `ExtendedCommandSettings`, and
[SimpleExec integration](../FileBasedApp.Toolkit.SimpleExec/README.md) for command runners.
Namespaces remain unchanged, but moving types to new assemblies is a **binary breaking
change**: recompile existing consumers after adding the package references. Core-only
consumers do not need either integration package. Deserializers and `FlexibleFilePath`
remain in core.

## Features

* Provides a `PathUtil` class for working with paths and validating string paths, including public static `TryGetFile`, `TryGetFiles`, `TryGetDirectory`, and `TryGetDirectories` helpers without a Spectre dependency
* Provides `LocalPathExtensions` with `IsHome`, `HasHomeDirectoryPrefix()`, and `ExpandHomeDirectory()` for home-directory expansion, `IsCurrentDirectory()` for lexical current-directory detection, plus `GetSegments()` and `Traverse()` for lexical path traversal
* Provides an IO class to provide methods and extension methods for `TruePath`
  * `FindInFiles` — filter files by stream predicate or regex (with `ByLine` / `AllText` strategies)
  * `Replace` — in-place file text transformation
  * `SafeDeleteDirectory` — safely delete directories with optional exception handling
* Provides `StringExtensions` with `StringJoin`, `IsNullOrWhitespace`, and `IsNullOrEmpty` convenience methods
* Includes the following libraries
  * `TruePath` 2.0.0
  * `TestableIO.System.IO.Abstractions.Wrappers`
  * `TruePath.TestableIO.System.IO` 1.1.0

## Example

### Path resolution and validation

Use `PathUtil.TryGetFile` / `TryGetDirectory` for a single `AbsolutePath`, or
`TryGetFiles` / `TryGetDirectories` for an `AbsolutePath[]`. These helpers are in the
core package and do not require `ExtendedCommandSettings` or Spectre.Console.

```csharp
using FileBasedApp.Toolkit;
using TruePath;

AbsolutePath directory = PathUtil.TryGetDirectory(
    "input", allowEmpty: false, shouldExist: true,
    root: PredefinedRootPath.CurrentDirectory);

AbsolutePath[] files = PathUtil.TryGetFiles(
    ["settings.json", "defaults.json"], shouldExist: true);

AbsolutePath[] directories = PathUtil.TryGetDirectories(
    ["input", "output"], allowEmpty: false, shouldExist: true,
    root: PathUtil.GetCurrentWorkingFolder());
```

Directory overloads and batch file overloads accept a predefined root or a single
absolute root. The `params` overloads accept multiple roots; omitted roots default
to the current working directory. `shouldExist` controls existence validation.

Despite their `TryGet` names, these methods throw on invalid input rather than
returning a Boolean. Null candidate arrays are rejected, as are empty file arrays.
An empty directory array is treated as a single empty candidate: `allowEmpty`
controls whether it may resolve to a root. Validation errors for multiple candidates
are aggregated into an `InvalidOperationException`.

The protected `TryGet*` methods in `ExtendedCommandSettings` remain backward-compatible
wrappers over these helpers, including their caller-argument expressions. Spectre's
`Validate()` error conversion remains in the optional integration package.

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

Traversal preserves absolute roots, Windows roots without a drive, and relative parent markers (`..`). A rooted-without-drive path such as `\foo\bar` has segments `\`, `foo`, and `bar`. Drive-relative prefixes remain attached to the first segment: `C:foo\bar` has segments `C:foo` and `bar`, while `C:` remains a single segment. Combining these segments in root-first order with `/` reconstructs the normalized path without resolving its drive or current-directory dependencies. Windows-only regression tests assert these segment and reconstruction contracts; cross-platform test execution remains unverified.

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

### Web URIs (AbsoluteWebUri / RelativeWebUri)

Strongly-typed, immutable wrappers around `System.Uri` for composing web URIs. Path segments, query strings, and fragments are represented as validated value objects (`UriPathSegment`, `UriQueryString`, `UriFragment`) and can be combined fluently via the `/` operator.

```csharp
using FileBasedApp.Toolkit;

// Build an absolute URI fluently
var url = AbsoluteWebUri.Create("https://example.com")
          / UriPathSegment.From("first")
          / UriPathSegment.From("second")
          / UriQueryString.From("a=1&b=2")
          / UriFragment.From("Fragment");

// https://example.com/first/second?a=1&b=2#Fragment
Console.WriteLine(url.ToString());

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
