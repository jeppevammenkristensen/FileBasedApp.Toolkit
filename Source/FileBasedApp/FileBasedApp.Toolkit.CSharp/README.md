# FileBasedApp.Toolkit.CSharp

Roslyn-based helpers for detecting and working with file-based .NET apps.

## Features

* Provides `FileBasedAppEvaluator` — inspects C# source files using Roslyn to determine if they are file-based apps
* Detects file-based directives in leading trivia: `#:package`, `#:property`, `#:sdk`, `#:project`
* Supports `IFileSystem` injection for testable file reads
* Includes both synchronous and asynchronous evaluation
* Provides `CsharpProjectAnalysis` for loading and analysing C# projects via MSBuild/Roslyn workspaces
* Provides `CompilationExtensions` for querying compilations — find type symbols, enumerate named types, and discover interface implementations
* Provides `CompilationWrapper` with cached type symbol lookups
* Provides `RoslynExtensions` for classifying `ITypeSymbol` as string-like, task-like, or enumerable
* Provides `FileEvaluationExtensions` for convenient `AbsolutePath`-based file-based app detection

## Example

### Detecting a file-based app

```csharp
using FileBasedApp.Toolkit.CSharp;
using TruePath;

var evaluator = new FileBasedAppEvaluator();
var path = AbsolutePath.Create("/path/to/my-script.cs");

if (evaluator.IsFileBasedApp(path))
{
    Console.WriteLine("This is a file-based app.");
}
```

A file is considered a file-based app when it is a top-level script (i.e. its first member is a global statement) and its leading trivia contains at least one recognised directive:

```csharp
#:package FileBasedApp.Toolkit@0.17.0
#:property PublishAot=false

Console.WriteLine("Hello from a file-based app!");
```

### Async evaluation

```csharp
bool result = await evaluator.IsFileBasedAppAsync(path, cancellationToken: token);
```

### Skipping the global statement check

By default, only top-level script files are considered. Pass `requireGlobalStatement: false` to evaluate any C# file regardless of structure:

```csharp
bool result = evaluator.IsFileBasedApp(path, requireGlobalStatement: false);
```

### Using a custom `IFileSystem`

```csharp
using System.IO.Abstractions;

IFileSystem fileSystem = new MockFileSystem(); // e.g. from TestableIO
var evaluator = new FileBasedAppEvaluator(fileSystem);
```

### Loading and analysing a C# project

```csharp
using FileBasedApp.Toolkit.CSharp;

await using var analysis = new CsharpProjectAnalysis();
await analysis.Load("/path/to/MyProject.csproj");

// Access the Roslyn Compilation
var compilation = analysis.Compilation;
```

### Finding interface implementations

```csharp
using FileBasedApp.Toolkit.CSharp.Extensions;

// Find all types implementing a specific interface in the current assembly
var implementations = compilation.FindImplementationOfInterface(
    "MyNamespace.IMyInterface", assemblyOnly: true);

foreach (var type in implementations)
{
    Console.WriteLine(type.Name);
}
```

### Type classification helpers

```csharp
using FileBasedApp.Toolkit.CSharp.Extensions;

// Check if a type symbol is string-like (string or ReadOnlySpan<char>)
StringInfo info = typeSymbol.IsStringLike(compilation);
if (info.IsStringLike) { /* ... */ }

// Check if a type is Task-like (Task, Task<T>, ValueTask, ValueTask<T>)
bool isAsync = typeSymbol.IsTaskLike(compilation);

// Check if a type is enumerable and get the element type
EnumerableInfo enumInfo = typeSymbol.TryGetEnumerableElementType(compilation);
if (enumInfo.IsEnumerable)
{
    Console.WriteLine($"Element type: {enumInfo.ElementType.Name}");
}
```

## Bugs or things missing

Feel free to create an issue or submit a pull request.

## Credits

[Floppy disc icons created by IYAHICON - Flaticon](https://www.flaticon.com/free-icons/floppy-disc "floppy disc icons")
