using System;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(RelativeWebUri))]
public class RelativeWebUriTest
{
    [Theory]
    [InlineData("/first/second", "/first/second")]
    [InlineData("relative/path", "/relative/path")]
    [InlineData("/path?q=1", "/path?q=1")]
    [InlineData("/path#frag", "/path#frag")]
    public void Create_ValidRelativeUri_ReturnsInstance(string input, string expected)
    {
        var result = RelativeWebUri.Create(input);
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_AbsoluteUri_ThrowsArgumentException()
    {
        var act = () => RelativeWebUri.Create("https://dr.dk/path");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_ValidRelativeUri_ReturnsInstance()
    {
        var result = RelativeWebUri.Parse("/some/path", null);
        result.Value.Should().Be("/some/path");
    }

    [Fact]
    public void Parse_InvalidUri_ThrowsFormatException()
    {
        var act = () => RelativeWebUri.Parse("https://dr.dk", null);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryParse_ValidRelativeUri_ReturnsTrueAndInstance()
    {
        var success = RelativeWebUri.TryParse("/path", null, out var result);
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Value.Should().Be("/path");
    }

    [Theory]
    [InlineData(null)]
    public void TryParse_Null_ReturnsFalse(string? input)
    {
        var success = RelativeWebUri.TryParse(input, null, out var result);
        success.Should().BeFalse();
        result.Should().BeNull();
    }
}
