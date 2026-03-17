# FileBasedApp.Toolkit.CSharp

Roslyn-based helpers for detecting and working with file-based .NET apps.

## Features

* Provides `FileBasedAppEvaluator` — inspects C# source files using Roslyn to determine if they are file-based apps
* Detects file-based directives in leading trivia: `#:package`, `#:property`, `#:sdk`, `#:project`
* Supports `IFileSystem` injection for testable file reads
* Includes both synchronous and asynchronous evaluation

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

## Bugs or things missing

Feel free to create an issue or submit a pull request.
