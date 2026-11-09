using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>
/// Guards the core assembly's independence from optional integrations.
/// </summary>
public class AssemblyDependencyTest
{
    [Fact]
    public void CoreAssembly_DoesNotReferenceSimpleExecOrSpectre()
    {
        var references = typeof(PathUtil).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty);

        references.Should().NotContain(name =>
            name.Contains("SimpleExec", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Spectre", StringComparison.OrdinalIgnoreCase));
    }
}
