#if NET11_0_OR_GREATER
using System;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>
/// Verifies the result-returning path APIs and their validation rules.
/// </summary>
[Collection("Sequential")]
public class PathUtilResultTest
{
    [Fact]
    public void GetFileResult_RequiredFileIsFoundInSecondRoot()
    {
        var first = AbsolutePath.CurrentWorkingDirectory / "first";
        var second = AbsolutePath.CurrentWorkingDirectory / "second";
        var expected = second / "sample.txt";
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(expected.Value, new MockFileData("sample"));
        using var scope = new FileSystemSetter<PathUtil>(fileSystem);

        var result = PathUtil.GetFileResult("sample.txt", true, first, second);

        GetValue(result).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetFileResult_BlankCandidateReturnsError(string? candidatePath)
    {
        var result = PathUtil.GetFileResult(candidatePath, shouldExist: false);

        GetError(result).Should().Be("Candidate path cannot be empty");
    }

    [Fact]
    public void GetFilesResult_PreservesCandidateOrderAndUsesPredefinedRoot()
    {
        var root = AbsolutePath.CurrentWorkingDirectory;
        using var scope = new FileSystemSetter<PathUtil>(new MockFileSystem());

        var result = PathUtil.GetFilesResult(["b.txt", "a.txt"], false, PredefinedRootPath.CurrentDirectory);

        GetValue(result).Should().Equal(root / "b.txt", root / "a.txt");
    }

    [Fact]
    public void GetFilesResult_ReturnsAllErrorsRatherThanPartialPaths()
    {
        var root = AbsolutePath.CurrentWorkingDirectory / "files";
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile((root / "present.txt").Value, new MockFileData("sample"));
        using var scope = new FileSystemSetter<PathUtil>(fileSystem);

        var result = PathUtil.GetFilesResult(["present.txt", "", "missing.txt"], true, root);

        GetError(result).Should().Be("2 files: Candidate path cannot be empty, No valid match");
    }

    [Fact]
    public void GetFilesResult_NullAndEmptyBatchesReturnErrors()
    {
        GetError(PathUtil.GetFilesResult(null, shouldExist: false)).Should().Be("Paths cannot be empty");
        GetError(PathUtil.GetFilesResult([], shouldExist: false)).Should().Be("Paths cannot be empty");
    }

    [Fact]
    public void GetDirectoriesResult_EmptyBatchResolvesToRootOnlyWhenAllowed()
    {
        var root = AbsolutePath.CurrentWorkingDirectory / "directories";
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(root.Value);
        using var scope = new FileSystemSetter<PathUtil>(fileSystem);

        GetValue(PathUtil.GetDirectoriesResult([], true, true, root)).Should().Equal(root);
        GetError(PathUtil.GetDirectoriesResult([], false, false, root)).Should().Be("OriginalPath cannot be null");
        GetError(PathUtil.GetDirectoriesResult(null, true, true, root)).Should()
            .Be("Directory candidates cannot be null");
    }

    [Fact]
    public void GetDirectoriesResult_AggregatesMissingDirectoryErrors()
    {
        var root = AbsolutePath.CurrentWorkingDirectory / "directories";
        using var scope = new FileSystemSetter<PathUtil>(new MockFileSystem());

        var result = PathUtil.GetDirectoriesResult(["first", "second"], false, true, root);

        GetError(result).Should().Be("2 directories: No valid match, No valid match");
    }

    private static T GetValue<T>(Result<T, ErrorString> result) =>
        result is T value ? value : throw new InvalidOperationException("Expected a successful path result.");

    private static string GetError<T>(Result<T, ErrorString> result) => result switch
    {
        ErrorString error => error.Error,
        _ => throw new InvalidOperationException("Expected a path validation error.")
    };
}
#endif