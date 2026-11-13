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
    public void TryGetPaths_ResolveCandidatesAgainstSuppliedRoot()
    {
        var root = AbsolutePath.CurrentWorkingDirectory / "helpers";
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(root.Value);
        fileSystem.AddFile((root / "test.json").Value, new MockFileData("{}"));

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            PathUtil.TryGetDirectory(".", false, true, root).Should().Be(root);
            PathUtil.TryGetDirectories([".", "."], false, true, root).Should().Equal(root, root);
            PathUtil.TryGetFile((root / "test.json").Value, true, root).Should().Be(root / "test.json");
            PathUtil.TryGetFiles([(root / "test.json").Value], true, root).Should().Equal(root / "test.json");
        }
    }

    [Fact]
    public void TryGetDirectories_EmptyCandidatesResolveToRootWhenAllowed()
    {
        var root = AbsolutePath.CurrentWorkingDirectory / "helpers";

        PathUtil.TryGetDirectories([], true, false, root).Should().Equal(root);
    }

    [Fact]
    public void TryGetFiles_EmptyCandidatesThrow()
    {
        var act = () => PathUtil.TryGetFiles([], false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryGetDirectories_MultipleInvalidCandidatesAggregateErrors()
    {
        using (new FileSystemSetter<PathUtil>(new MockFileSystem()))
        {
            var act = () => PathUtil.TryGetDirectories(["missing-one", "missing-two"], false, true,
                paramName: "directories");

            act.Should().Throw<InvalidOperationException>().WithMessage("directories: 2 directories: *");
        }
    }

    [Fact]
    public void TryGetFile_EmptyCandidateIncludesCallerExpressionInError()
    {
        var candidate = "";
        var act = () => PathUtil.TryGetFile(candidate, false);

        act.Should().Throw<InvalidOperationException>().WithMessage("candidate: Candidate path cannot be empty");
    }

    [WindowsTheory]
    [InlineData(false, @"\logs", @"C:\logs")]
    [InlineData(true, @"\logs", @"C:\logs")]
    [InlineData(false, "C:logs", @"C:\base\logs")]
    [InlineData(true, "C:logs", @"C:\base\logs")]
    [InlineData(false, "C:", @"C:\base")]
    [InlineData(true, "C:", @"C:\base")]
    [InlineData(false, @"D:\logs", @"D:\logs")]
    [InlineData(true, @"D:\logs", @"D:\logs")]
    public void AnalyzePath_WindowsInput_ResolvesAgainstSuppliedRoot(bool directory, string candidate, string expected)
    {
        var fileSystem = new MockFileSystem();
        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var root = new AbsolutePath(@"C:\base");
            var result = directory
                ? PathUtil.AnalyzeDirectory(candidate, root)
                : PathUtil.AnalyzeFile(candidate, root);

            AssertPath(result, false, false, new AbsolutePath(expected));
        }
    }

    [WindowsTheory]
    [InlineData(false, "D:logs")]
    [InlineData(true, "D:logs")]
    [InlineData(false, "D:")]
    [InlineData(true, "D:")]
    public void AnalyzePath_CrossDriveRelativeInput_UsesProcessDriveDirectoryInsteadOfSuppliedRoot(bool directory,
        string candidate)
    {
        // TruePath uses real process drive-directory state, not the mock file system's current directory.
        var processFileSystem = new System.IO.Abstractions.FileSystem();
        var expected = new AbsolutePath(processFileSystem.Path.GetFullPath(candidate));
        var fileSystem = new MockFileSystem();
        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            foreach (var rootValue in new[] {@"C:\base", @"C:\other"})
            {
                var root = new AbsolutePath(rootValue);
                var result = directory
                    ? PathUtil.AnalyzeDirectory(candidate, root)
                    : PathUtil.AnalyzeFile(candidate, root);

                AssertPath(result, false, false, expected);
            }
        }
    }

    /// <summary>Skips Windows path-resolution cases on platforms with different path semantics.</summary>
    private sealed class WindowsTheoryAttribute : TheoryAttribute
    {
        public WindowsTheoryAttribute()
        {
            if (!OperatingSystem.IsWindows())
            {
                Skip = "Requires Windows drive-relative and rooted path semantics.";
            }
        }
    }


    [Fact]
    public void AnalyzeFilePath_MultipleRootsNoMatch_FileShouldNotExist()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("test.file", AbsolutePath.CurrentWorkingDirectory / "first",
                AbsolutePath.CurrentWorkingDirectory / "second");
            var errorMessage = AssertError(analyzeDirectory, true, false);
#if NET11_0_OR_GREATER
            errorMessage.Should().Be("No valid match");
#else
            errorMessage.Should().Contain("does not exist");
#endif
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
            var analyzeDirectory = PathUtil.AnalyzeFile("test.file", AbsolutePath.CurrentWorkingDirectory / "first",
                AbsolutePath.CurrentWorkingDirectory / "second");
            AssertPath(analyzeDirectory, true, false, AbsolutePath.CurrentWorkingDirectory / "second" / "test.file");
        }
    }

    [Theory]
    [InlineData(false, false, "first")]
    [InlineData(true, false, "second")]
    [InlineData(true, true, "first")]
    public void AnalyzeFilePath_MultipleRoots_SelectsFirstEligiblePath(bool shouldExist, bool firstExists,
        string expectedRoot)
    {
        var root = AbsolutePath.CurrentWorkingDirectory;
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile((root / "second" / "test.file").Value, new MockFileData("test"));
        if (firstExists)
        {
            fileSystem.AddFile((root / "first" / "test.file").Value, new MockFileData("test"));
        }

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var evaluated = PathUtil.AnalyzeFile("test.file", root / "first", root / "second");

            AssertPath(evaluated, shouldExist, false, root / expectedRoot / "test.file");
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
            AssertPath(analyzeDirectory, true, false, AbsolutePath.CurrentWorkingDirectory / "test.file");
        }
    }

    [Fact]
    public void AnalyzeFilePath_FileDoesNotExist_ShouldNotRequireExist()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile("nonexistent.file");
            AssertPath(analyzeDirectory, false, false, AbsolutePath.CurrentWorkingDirectory / "nonexistent.file");
        }
    }

    [Fact]
    public void AnalyzeFilePath_NullPath_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeFile(null);
            AssertError(analyzeDirectory, false, false).Should().Be("OriginalPath cannot be null");
        }
    }

    [Fact]
    public void AnalyzeDirectory_WithExistingDirectory_ShouldReturnPath()
    {
        var testdirectory = AbsolutePath.CurrentWorkingDirectory / "existing";

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {testdirectory.Value, new MockFileData("")}
        });
        fileSystem.AddDirectory(testdirectory.Value);

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeDirectory("existing", AbsolutePath.CurrentWorkingDirectory);
            AssertPath(analyzeDirectory, true, false, testdirectory);
        }
    }

    [Fact]
    public void AnalyzeDirectory_WithNonExistingDirectory_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory =
                PathUtil.AnalyzeDirectory("nonexistent", AbsolutePath.CurrentWorkingDirectory / "test");
            var errorMessage = AssertError(analyzeDirectory, true, false);
#if NET11_0_OR_GREATER
            errorMessage.Should().Be("No valid match");
#else
            errorMessage.Should().Contain("does not exist");
#endif
        }
    }

    [Fact]
    public void AnalyzeDirectory_EmptyPath_ShouldReturnRoot()
    {
        var fileSystem = new MockFileSystem();

        using (new FileSystemSetter<PathUtil>(fileSystem))
        {
            var analyzeDirectory = PathUtil.AnalyzeDirectory("", AbsolutePath.CurrentWorkingDirectory / "test");
            AssertPath(analyzeDirectory, false, true, AbsolutePath.CurrentWorkingDirectory / "test");
        }
    }

    /// <summary>Checks the success contract for both union and tuple target frameworks.</summary>
    private static void AssertPath(IEvaluatedPath evaluated, bool shouldExist, bool originalPathCanBeNull,
        AbsolutePath expected)
    {
        var result = evaluated.GetPath(shouldExist, originalPathCanBeNull);
#if NET11_0_OR_GREATER
        result.Valid.Should().BeTrue();
        Assert.True(result is AbsolutePath);
        result.GetPathOrThrow(message => new InvalidOperationException(message)).Should().Be(expected);
#else
        result.errorMessage.Should().BeNull();
        result.path.Should().Be(expected);
#endif
    }

    /// <summary>Checks the error contract and returns its message for scenario-specific assertions.</summary>
    private static string AssertError(IEvaluatedPath evaluated, bool shouldExist, bool originalPathCanBeNull)
    {
        var result = evaluated.GetPath(shouldExist, originalPathCanBeNull);
#if NET11_0_OR_GREATER
        result.Valid.Should().BeFalse();
        Assert.True(result is string);
        return result switch
        {
            string message => message,
            AbsolutePath _ => throw new InvalidOperationException("Expected a path evaluation error.")
        };
#else
        result.path.Should().Be(default(AbsolutePath));
        return Assert.IsType<string>(result.errorMessage);
#endif
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
            var act = () => PathUtil.GetRootFolder((PredefinedRootPath) 999);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
