using System;
using FileBasedApp.Toolkit.CommandCli;
using FluentAssertions;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>Verifies platform selection and lexical conversion without requiring absolute paths or accessing disk.</summary>
public class FlexibleFilePathTest
{
    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("a/..")]
    [InlineData("..")]
    [InlineData("folder/file.txt")]
    [InlineData("~/file.txt")]
    public void Conversions_PreserveRelativePathsWithoutResolvingThem(string value)
    {
        var expected = new LocalPath(value);

        AssertConversions(new FlexibleFilePath(value, value), expected);
        expected.IsAbsolute.Should().BeFalse();
    }

    [Fact]
    public void Conversions_SelectCurrentPlatformPath()
    {
        var flexible = new FlexibleFilePath("windows/file.txt", "unix/file.txt");
        var expected = new LocalPath(OperatingSystem.IsWindows() ? flexible.Windows : flexible.Unix);

        AssertConversions(flexible, expected);
    }

    [Fact]
    public void Conversions_PreserveAbsolutePaths()
    {
        var expected = AbsolutePath.CurrentWorkingDirectory / "file.txt";

        AssertConversions(new FlexibleFilePath(expected.Value, expected.Value), expected);
    }

    [WindowsTheory]
    [InlineData(@"\foo", false)]
    [InlineData(@"\", false)]
    [InlineData("C:foo", false)]
    [InlineData("C:", false)]
    [InlineData(@"C:\foo", true)]
    public void Conversions_WindowsPaths_PreserveRootAndDriveSemantics(string value, bool isAbsolute)
    {
        var expected = new LocalPath(value);

        AssertConversions(new FlexibleFilePath(value, "unix/unselected"), expected);
        expected.IsAbsolute.Should().Be(isAbsolute);
    }

    /// <summary>Checks explicit and implicit conversions preserve the expected normalized path.</summary>
    private static void AssertConversions(FlexibleFilePath flexible, LocalPath expected)
    {
        flexible.GetAsLocalPath().Should().Be(expected);
        LocalPath converted = flexible;
        converted.Should().Be(expected);
    }

    /// <summary>Skips drive-specific cases on platforms with different path semantics.</summary>
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
}