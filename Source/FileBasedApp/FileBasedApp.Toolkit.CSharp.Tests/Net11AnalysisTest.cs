#if NET11_0
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FileBasedApp.Toolkit;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

/// <summary>Verifies real .NET 11 project and solution loading through the MSBuild BuildHost.</summary>
public class Net11AnalysisTest
{
    [Fact]
    public async Task LoadProject_ResolvesNet11FrameworkAndReferences()
    {
        await using var analysis = new CSharpProjectAnalysis(AnsiConsoleFactory.Quiet());

        await analysis.Load(FixtureDirectory() / "Net11AnalysisFixture.csproj");

        AssertNet11Compilation(analysis.Project, analysis.Compilation);
        analysis.Workspace.Diagnostics.Where(d => d.Kind == WorkspaceDiagnosticKind.Failure)
            .Should().BeEmpty();
    }

    [Fact]
    public async Task LoadSolution_ResolvesNet11FrameworkAndReferences()
    {
        await using var analysis = new CSharpSolutionAnalysis(AnsiConsoleFactory.Quiet());

        await analysis.Load(FixtureDirectory() / "Net11AnalysisFixture.slnx");

        var project = analysis.Solution.Projects.Should().ContainSingle().Subject;
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();
        AssertNet11Compilation(project, compilation!);
        analysis.Workspace.Diagnostics.Where(d => d.Kind == WorkspaceDiagnosticKind.Failure)
            .Should().BeEmpty();
    }

    /// <summary>Locates the checked-in fixture independently of the test output directory.</summary>
    private static AbsolutePath FixtureDirectory() => PathUtil.GetCurrentWorkingFolder()
        .FindRequiredParent(path => (path / "root.marker").FileExists())
        / "Playground" / "Net11AnalysisFixture";

    /// <summary>Checks evaluated framework symbols, framework references, and compilation errors.</summary>
    private static void AssertNet11Compilation(Project project, Compilation compilation)
    {
        project.ParseOptions.Should().BeOfType<CSharpParseOptions>()
            .Which.PreprocessorSymbolNames.Should().Contain("NET11_0");
        compilation.GetSpecialType(SpecialType.System_Object).ContainingAssembly.Identity.Version.Major
            .Should().Be(11);
        compilation.GetTypeByMetadataName("Net11AnalysisFixture.FrameworkProbe")
            .Should().NotBeNull();
        compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();
    }
}
#endif
