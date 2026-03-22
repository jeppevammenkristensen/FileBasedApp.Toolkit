using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

public partial class CsharpProjectAnalysisFullLoadTest
{

    [Fact]
    public void FindInterfaceByName_Found_ReturnsExpected()
    {
        var matches = _analysis.FindImplementationOfInterface("Test.Project.ISomeInterface", true).ToList();
        matches.Should().HaveCount(1);
        var expected = _analysis.Compilation.GetTypeByMetadataName("Test.Project.Someclass");
        matches[0].Should().BeEquivalentTo(expected, options => options.Using(SymbolEqualityComparer.Default),"the interface should be implemented by Test.Project.Someclass");
    }
    
    [Fact]
    public void FindInterfaceByName_NotFound_ShouldThrowAnException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _analysis.FindImplementationOfInterface("Test.Project.ISomeInterfac", true));
    }
    
    [Fact]
    public void FindInterfaceByName_NotInterface_ShouldThrowAnException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _analysis.FindImplementationOfInterface("Test.Project.Someclass", true));
    }
}