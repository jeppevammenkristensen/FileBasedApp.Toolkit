using System;
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

    [Fact]
    public void AnalyzeFilePath_FileDoesNotExist_ShouldNotRequireExist()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("nonexistent.file");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: false, false);
            errorMessage.Should().BeNull();
            path.Should().NotBeNull();
        }
    }

    [Fact]
    public void AnalyzeFilePath_NullPath_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile(null);
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: false, false);
            errorMessage.Should().NotBeNull();
            path.Should().BeEquivalentTo(default(AbsolutePath));
        }
    }

    [Fact]
    public void AnalyzeDirectory_WithExistingDirectory_ShouldReturnPath()
    {
        var testdirectory = AbsolutePath.CurrentWorkingDirectory / "existing";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { testdirectory.Value, new MockFileData("") }
        });
        fileSystem.AddDirectory(testdirectory.Value);

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeDirectory("existing", AbsolutePath.CurrentWorkingDirectory);
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: true, false);
            errorMessage.Should().BeNull();
            path.Should().BeEquivalentTo(testdirectory);
        }
    }

    [Fact]
    public void AnalyzeDirectory_WithNonExistingDirectory_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeDirectory("nonexistent",AbsolutePath.CurrentWorkingDirectory / "test");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: true, false);
            errorMessage.Should().NotBeNull();
            errorMessage.Should().Contain("does not exist");
        }
    }

    [Fact]
    public void AnalyzeDirectory_EmptyPath_ShouldReturnRoot()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeDirectory("", AbsolutePath.CurrentWorkingDirectory / "test");
            (AbsolutePath path, string errorMessage) = analyzeDirectory.GetPath(shouldExist: false, true);
            errorMessage.Should().BeNull();
            path.Should().BeEquivalentTo(AbsolutePath.CurrentWorkingDirectory / "test");
        }
    }

    [Fact]
    public void GetRootFolder_CurrentDirectory_ShouldReturnCurrentWorkingDirectory()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var root = PathUtil.GetRootFolder(PredefinedRootPath.CurrentDirectory);
            root.Should().Be(AbsolutePath.CurrentWorkingDirectory);
        }
    }

    [Fact]
    public void GetRootFolder_InvalidPredefinedRootPath_ShouldThrow()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var act = () => PathUtil.GetRootFolder((PredefinedRootPath)999);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}