using System;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

// Note this test works on the Test.Project that lives in the Playground Folder

public class CsharpProjectAnalysisTest
{
    [Fact]
    public void AccessPropertiesBeforeLoadThrowsError()
    {
        var csharpProjectAnalysis = CsharpProjectAnalysis.Init;
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Project);
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Compilation);
        Assert.Throws<InvalidOperationException>(() => csharpProjectAnalysis.Workspace);
    }
}