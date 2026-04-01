using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FileBasedApp.Toolkit.CSharp.Extensions;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(CSharpProjectAnalysis))]
// [Trait("Category", "Slow")]
public partial class CSharpProjectAnalysisFullLoadTest : IAsyncLifetime
{
    private CSharpProjectAnalysis _analysis;

    [Fact]
    public void PropertiesLoadedCorrectOnExistingProject()
    {
        _analysis.Compilation.Should().NotBeNull();
        _analysis.Project.Should().NotBeNull();
        _analysis.Workspace.Should().NotBeNull();
    }

    public const int UniqueNamedTypesInTestProject = 2;
    
    
    [Fact]
    public void GetNamedTypeSymbols_ReturnsExpected()
    {
        var matches = _analysis.Compilation.GetNamedTypeSymbolsIncludingReferences()
            .ToList();
        
        matches.Should().HaveCountGreaterThan(UniqueNamedTypesInTestProject);
        matches.Where(x => SymbolEqualityComparer.Default.Equals(x.ContainingAssembly, _analysis.Compilation.Assembly))
            .Should().HaveCount(UniqueNamedTypesInTestProject, $"There are {UniqueNamedTypesInTestProject} unique named types in the test project. If this has changed update the {nameof(UniqueNamedTypesInTestProject)} constant");
    }
    
    [Fact]
    public void GetNamedTypeSymbolsForCurrentAssembly_ReturnsExpected()
    {
        var matches = _analysis.Compilation.GetNamedTypeSymbolForCurrentAssembly()
            .ToList();
        
        matches.Should().HaveCount(UniqueNamedTypesInTestProject, $"There are {UniqueNamedTypesInTestProject} unique named types in the test project. If this has changed update the {nameof(UniqueNamedTypesInTestProject)} constant");
    }
    
    [Fact]
    public void FindImplementationOfInterface_ReturnsExpected()
    {
        var matches = _analysis.Compilation.GetNamedTypeSymbolForCurrentAssembly()
            .ToList();

        var @if = matches.SingleOrDefault(x => x.Name == "ISomeInterface");

        matches
            .Where(x => !x.Equals(@if, SymbolEqualityComparer.Default))
            .Where(x => x.AllInterfaces.Contains(@if, SymbolEqualityComparer.Default))
            .Should().HaveCount(1).And.Subject.First().Name.Should().Be("Someclass");
        
    }
    
    
    
    /// <summary>
    /// From the current path traverses up until it finds a folder that has a
    /// file named root.marker. This is used to identify the root of the project.
    /// </summary>
    /// <returns></returns>
    private AbsolutePath FindProjectRoot()
    {
        return PathUtil.GetCurrentWorkingFolder().FindRequiredParent(x => (x / "root.marker").FileExists());
    }

    public async Task InitializeAsync()
    {
        var project = FindProjectRoot() / "Playground" / "Test.Project" / "Test.Project.csproj";
        if (!project.FileExists())
        {
            throw new FileNotFoundException("Could not find test project at path", project.Value);
        }

        _analysis = await CSharpProjectAnalysis.Init.Load(project);
        
    }

    public async Task DisposeAsync()
    {
        await _analysis.DisposeAsync();
    }
}