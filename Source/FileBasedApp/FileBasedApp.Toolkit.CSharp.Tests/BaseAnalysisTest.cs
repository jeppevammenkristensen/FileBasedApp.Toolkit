using FluentAssertions;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

public class BaseAnalysisTest
{
    [Fact]
    public void WithAnsiConsole_SetsConsole()
    {
        var dummy = new DummyAnalysis(AnsiConsoleFactory.Quiet());
        var newConsole = AnsiConsoleFactory.Quiet();

        var result = dummy.WithAnsiConsole(newConsole);

        result.Should().BeSameAs(dummy);
        dummy.ExposedConsole.Should().BeSameAs(newConsole);
    }
}
