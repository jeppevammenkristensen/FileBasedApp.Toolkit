using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp.Tests;

/// <summary>
/// Minimal <see cref="BaseAnalysis{TSelf}"/> subclass used to exercise base-class behavior (e.g. console
/// injection via <see cref="BaseAnalysis{TSelf}.WithAnsiConsole"/>) that isn't reachable through the public API
/// of the concrete analysis classes, since <see cref="BaseAnalysis{TSelf}.Console"/> is protected.
/// </summary>
internal sealed class DummyAnalysis : BaseAnalysis<DummyAnalysis>
{
    public DummyAnalysis(IAnsiConsole console, IFileSystem? fileSystem = null)
        : base(console, fileSystem ?? new FileSystem())
    {
    }

    public IAnsiConsole ExposedConsole => Console;

    protected internal override Task InternalLoad(AbsolutePath path, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    protected override void EnsureCorrectlyLoadedExtraChecks()
    {
    }
}
