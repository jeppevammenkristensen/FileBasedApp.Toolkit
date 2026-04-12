using System;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(UriExtensions))]
public class UriExtensionsTest
{
    // --- SafeParseUri ---

    [Fact]
    public void SafeParseUri_ValidUri_ReturnsUri()
    {
        var result = "https://example.com/path".SafeParseUri();
        result.Should().NotBeNull();
        result!.AbsoluteUri.Should().Be("https://example.com/path");
    }

    [Fact]
    public void SafeParseUri_Null_ReturnsNull()
    {
        string? input = null;
        input.SafeParseUri().Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a uri")]
    [InlineData("::::::")]
    public void SafeParseUri_Invalid_ReturnsNull(string input)
    {
        input.SafeParseUri().Should().BeNull();
    }

    // --- ToQueryString ---

    [Fact]
    public void ToQueryString_Valid_ReturnsValueObject()
    {
        var result = "a=1&b=2".ToQueryString();
        result!.Value.Should().Be("?a=1&b=2");
    }

    [Fact]
    public void ToQueryString_Null_ReturnsNull()
    {
        string? input = null;
        input.ToQueryString().Should().BeNull();
    }

    [Fact]
    public void ToRequiredQueryString_Valid_ReturnsValueObject()
    {
        var result = "a=1".ToRequiredQueryString();
        result.Value.Should().Be("?a=1");
    }

    [Fact]
    public void ToRequiredQueryString_Null_Throws()
    {
        string? input = null;
        var act = input.ToRequiredQueryString;
        act.Should().Throw<ArgumentNullException>();
    }

    // --- ToFragment ---

    [Fact]
    public void ToFragment_Valid_ReturnsValueObject()
    {
        var result = "section".ToFragment();
        result!.Value.Should().Be("#section");
    }

    [Fact]
    public void ToFragment_Null_ReturnsNull()
    {
        string? input = null;
        input.ToFragment().Should().BeNull();
    }

    [Fact]
    public void ToRequiredFragment_Valid_ReturnsValueObject()
    {
        var result = "top".ToRequiredFragment();
        result.Value.Should().Be("#top");
    }

    [Fact]
    public void ToRequiredFragment_Null_Throws()
    {
        string? input = null;
        var act = () => input.ToRequiredFragment();
        act.Should().Throw<ArgumentNullException>();
    }

    // --- ToPathSegment ---

    [Fact]
    public void ToPathSegment_Valid_ReturnsValueObject()
    {
        var result = "users".ToPathSegment();
        result!.Value.Should().Be("users");
    }

    [Fact]
    public void ToPathSegment_Null_ReturnsNull()
    {
        string? input = null;
        input.ToPathSegment().Should().BeNull();
    }

    [Fact]
    public void ToRequiredPathSegment_Valid_ReturnsValueObject()
    {
        var result = "users".ToRequiredPathSegment();
        result.Value.Should().Be("users");
    }
}
