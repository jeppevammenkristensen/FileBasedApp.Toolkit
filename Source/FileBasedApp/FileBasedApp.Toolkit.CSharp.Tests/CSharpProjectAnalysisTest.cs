using System;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

// Note this test works on the Test.Project that lives in the Playground Folder

public class CSharpProjectAnalysisTest
{
    [Fact]
    public void AccessPropertiesBeforeLoadThrowsError()
    {
        var csharpProjectAnalysis = CSharpProjectAnalysis.Init;
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Project);
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Compilation);
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Workspace);
    }
}