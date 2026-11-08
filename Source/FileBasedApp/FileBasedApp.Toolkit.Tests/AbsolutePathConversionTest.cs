using System;
using FluentAssertions;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>Verifies that absolute-path conversions validate input without resolving relative paths or accessing disk.</summary>
public class AbsolutePathConversionTest
{
    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("folder/file.txt")]
    public void Conversions_RejectRelativeOrEmptyInput(string value)
    {
        AssertRejected(value);
    }

    [WindowsTheory]
    [InlineData(@"\foo")]
    [InlineData("C:foo")]
    [InlineData("C:")]
    public void Conversions_WindowsPaths_RejectInputsThatAreNotFullyQualified(string value)
    {
        AssertRejected(value);
    }

    [Fact]
    public void Conversions_AcceptFullyQualifiedInput()
    {
        AssertAccepted(AbsolutePath.CurrentWorkingDirectory / "file.txt");
    }

    [WindowsTheory]
    [InlineData(@"C:\foo")]
    [InlineData(@"C:\")]
    public void Conversions_WindowsPaths_AcceptFullyQualifiedInput(string value)
    {
        AssertAccepted(new AbsolutePath(value));
    }

    [Fact]
    public void StringConversions_NullInput_IsOptionalOnlyForAsAbsolutePath()
    {
        string? value = null;

        value.AsAbsolutePath().Should().BeNull();
        Assert.Throws<ArgumentNullException>(() => value!.AsRequiredAbsolutePath());
    }

    /// <summary>Checks absolute-path conversion helpers reject the input rather than resolving it or returning null.</summary>
    private static void AssertRejected(string value)
    {
        Assert.Throws<ArgumentException>(() => value.AsAbsolutePath());
        Assert.Throws<ArgumentException>(() => value.AsRequiredAbsolutePath());
        Assert.Throws<ArgumentException>(() => value.AsSpan().AsAbsolutePath());
    }

    /// <summary>Checks fully qualified paths are preserved by each absolute-path conversion helper.</summary>
    private static void AssertAccepted(AbsolutePath expected)
    {
        expected.Value.AsAbsolutePath().Should().Be(expected);
        expected.Value.AsRequiredAbsolutePath().Should().Be(expected);
        expected.Value.AsSpan().AsAbsolutePath().Should().Be(expected);
    }

    /// <summary>Skips drive-specific validation cases on platforms with different path semantics.</summary>
    private sealed class WindowsTheoryAttribute : TheoryAttribute
    {
        public WindowsTheoryAttribute()
        {
            if (!OperatingSystem.IsWindows())
            {
                Skip = "Requires Windows fully qualified path semantics.";
            }
        }
    }
}