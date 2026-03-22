using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(CsharpProjectAnalysis))]
[Trait("Category", "Slow")]
public class CsharpProjectAnalysisFullLoadTest : IAsyncLifetime
{
    private CsharpProjectAnalysis _analysis;

    [Fact]
    public void PropertiesLoadedCorrectOnExistingProject()
    {
        _analysis.Compilation.Should().NotBeNull();
        _analysis.Project.Should().NotBeNull();
        _analysis.MsBuildWorkspace.Should().NotBeNull();
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

        _analysis = await CsharpProjectAnalysis.Init.Load(project);
        
    }

    public async Task DisposeAsync()
    {
        await _analysis.DisposeAsync();
    }
}