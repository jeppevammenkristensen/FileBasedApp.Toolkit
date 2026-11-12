#if NET11_0_OR_GREATER
using System;
using FluentAssertions;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>Verifies the .NET 11 path-or-error union and single-path evaluation contract.</summary>
public class PathOrErrorTest
{
    [Fact]
    public void GetPathOrThrow_ValidPath_ReturnsPathWithoutCallingFactory()
    {
        var expected = AbsolutePath.CurrentWorkingDirectory / "test.file";
        PathOrError result = expected;
        var factoryCalled = false;

        var actual = result.GetPathOrThrow(message =>
        {
            factoryCalled = true;
            return new InvalidOperationException(message);
        });

        result.Valid.Should().BeTrue();
        actual.Should().Be(expected);
        factoryCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData("Path does not exist")]
    [InlineData("")]
    public void GetPathOrThrow_Error_ThrowsExceptionCreatedFromMessage(string error)
    {
        PathOrError result = error;
        var expectedException = new ArgumentException(error);
        var factoryCalls = 0;
        string? receivedMessage = null;

        var act = () => result.GetPathOrThrow(message =>
        {
            factoryCalls++;
            receivedMessage = message;
            return expectedException;
        });

        result.Valid.Should().BeFalse();
        act.Should().Throw<ArgumentException>().Which.Should().BeSameAs(expectedException);
        factoryCalls.Should().Be(1);
        receivedMessage.Should().Be(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetPath_MissingOriginalPath_ReturnsErrorBeforeExistenceCheck(string? originalPath)
    {
        var evaluated = new EvaluatedPath(originalPath, null, false);

        var result = evaluated.GetPath(shouldExist: true, originalPathCanBeNull: false);

        AssertError(result, "OriginalPath cannot be null");
    }

    [Fact]
    public void GetPath_RequiredPathDoesNotExist_ReturnsError()
    {
        var path = AbsolutePath.CurrentWorkingDirectory / "missing.file";
        var evaluated = new EvaluatedPath("missing.file", path, false);

        var result = evaluated.GetPath(shouldExist: true, originalPathCanBeNull: false);

        AssertError(result, $"Path missing.file translated to {path} does not exist");
    }

    [Fact]
    public void GetPath_UntranslatedPath_ReturnsError()
    {
        var evaluated = new EvaluatedPath("invalid", null, false);

        var result = evaluated.GetPath(shouldExist: false, originalPathCanBeNull: false);

        AssertError(result, "Path invalid could not be translated to an AbsolutePath");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void GetPath_ValidPath_ReturnsAbsolutePath(bool shouldExist, bool exists)
    {
        var expected = AbsolutePath.CurrentWorkingDirectory / "test.file";
        var evaluated = new EvaluatedPath("test.file", expected, exists);

        var result = evaluated.GetPath(shouldExist, originalPathCanBeNull: false);

        result.Valid.Should().BeTrue();
        Assert.True(result is AbsolutePath);
        result.GetPathOrThrow(message => new InvalidOperationException(message)).Should().Be(expected);
    }

    /// <summary>Asserts the error case carries the exact evaluation message.</summary>
    private static void AssertError(PathOrError result, string expected)
    {
        result.Valid.Should().BeFalse();
        Assert.True(result is string);
        var message = result switch
        {
            string error => error,
            AbsolutePath _ => throw new InvalidOperationException("Expected an error, not a path.")
        };
        message.Should().Be(expected);
    }
}
#endif