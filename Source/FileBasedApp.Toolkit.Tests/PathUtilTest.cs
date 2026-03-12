using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[Collection("Sequential")]
[TestSubject(typeof(PathUtil))]
public class PathUtilTest
{

    [Fact]
    public void AnalyzeFilePath_MultipleRootsNoMatch_FileShouldNotExist()
    {
        var fileSystem = new MockFileSystem();
        
        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("test.file", AbsolutePath.CurrentWorkingDirectory / "first", AbsolutePath.CurrentWorkingDirectory / "second");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: true, false);
            errorMessage.Should().NotBeNull();
            path.Should().BeEquivalentTo(default(AbsolutePath));
        }
        
    }
    
    [Fact]
    public void AnalyzeFilePath_MultipleRootsSecondMatch_FileShouldNotExist()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            {(AbsolutePath.CurrentWorkingDirectory / "second" / "test.file").Value, new MockFileData("test")}
        });
        
        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("test.file", AbsolutePath.CurrentWorkingDirectory / "first", AbsolutePath.CurrentWorkingDirectory / "second");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: true, false);
            errorMessage.Should().BeNull();
            path.Should().BeEquivalentTo(AbsolutePath.CurrentWorkingDirectory / "second" / "test.file");
        }
    }
    
    [Fact]
    public void AnalyzeFilePath_NoRoots_FileShouldExists()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            {(AbsolutePath.CurrentWorkingDirectory / "test.file").Value, new MockFileData("test")}
        });

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("test.file");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: true, false);
            errorMessage.Should().BeNull();
            path.Should().BeEquivalentTo(AbsolutePath.CurrentWorkingDirectory / "test.file");
        }
    }
}