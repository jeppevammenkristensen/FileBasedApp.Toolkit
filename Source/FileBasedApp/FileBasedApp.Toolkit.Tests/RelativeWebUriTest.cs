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
    public void AddPathSegment_AppendsSegment()
    {
        var uri = RelativeWebUri.Create("/first");
        var result = uri.AddPathSegment("second");
        result.Value.Should().Be("/first/second");
    }

    [Fact]
    public void AddPathSegment_PreservesQueryAndFragment()
    {
        var uri = RelativeWebUri.Create("/first?q=1#frag");
        var result = uri.AddPathSegment("second");
        result.Value.Should().Be("/first/second?q=1#frag");
    }

    [Fact]
    public void AddPathSegment_WithTrailingSlash_DoesNotDoubleSlash()
    {
        var uri = RelativeWebUri.Create("/first/");
        var result = uri.AddPathSegment("second");
        result.Value.Should().Be("/first/second");
    }

    [Theory]
    [InlineData("section1", "/path#section1")]
    [InlineData("#section1", "/path#section1")]
    public void WithFragment_AddsFragment(string fragment, string expected)
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri.WithFragment(fragment);
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void WithFragment_ReplacesExistingFragment()
    {
        var uri = RelativeWebUri.Create("/path#old");
        var result = uri.WithFragment("new");
        result.Value.Should().Be("/path#new");
    }

    [Fact]
    public void WithFragment_PreservesQuery()
    {
        var uri = RelativeWebUri.Create("/path?q=1");
        var result = uri.WithFragment("frag");
        result.Value.Should().Be("/path?q=1#frag");
    }

    [Theory]
    [InlineData("key=value", "/path?key=value")]
    [InlineData("?key=value", "/path?key=value")]
    public void WithRawQuerystring_AddsQuerystring(string querystring, string expected)
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri.WithRawQuerystring(querystring);
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void WithRawQuerystring_ReplacesExistingQuery()
    {
        var uri = RelativeWebUri.Create("/path?old=1");
        var result = uri.WithRawQuerystring("new=2");
        result.Value.Should().Be("/path?new=2");
    }

    [Fact]
    public void WithRawQuerystring_PreservesFragment()
    {
        var uri = RelativeWebUri.Create("/path#frag");
        var result = uri.WithRawQuerystring("q=1");
        result.Value.Should().Be("/path?q=1#frag");
    }

    [Fact]
    public void AddQueryPart_AddsToEmptyQuery()
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri.AddQueryPart("key", "value");
        result.Value.Should().Be("/path?key=value");
    }

    [Fact]
    public void AddQueryPart_AppendsToExistingQuery()
    {
        var uri = RelativeWebUri.Create("/path?existing=1");
        var result = uri.AddQueryPart("key", "value");
        result.Value.Should().Contain("existing=1");
        result.Value.Should().Contain("key=value");
    }

    [Fact]
    public void AddQueryPart_PreservesFragment()
    {
        var uri = RelativeWebUri.Create("/path#frag");
        var result = uri.AddQueryPart("key", "value");
        result.Value.Should().Contain("key=value");
        result.Value.Should().EndWith("#frag");
    }

    [Fact]
    public void HasQuery_WithQuery_ReturnsTrue()
    {
        var uri = RelativeWebUri.Create("/path?q=1");
        uri.HasQuery.Should().BeTrue();
    }

    [Fact]
    public void HasQuery_WithoutQuery_ReturnsFalse()
    {
        var uri = RelativeWebUri.Create("/path");
        uri.HasQuery.Should().BeFalse();
    }

    [Fact]
    public void HasFragments_WithFragment_ReturnsTrue()
    {
        var uri = RelativeWebUri.Create("/path#frag");
        uri.HasFragments.Should().BeTrue();
    }

    [Fact]
    public void HasFragments_WithoutFragment_ReturnsFalse()
    {
        var uri = RelativeWebUri.Create("/path");
        uri.HasFragments.Should().BeFalse();
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

    [Fact]
    public void Operations_ReturnNewInstance()
    {
        var original = RelativeWebUri.Create("/path");
        var withSegment = original.AddPathSegment("new");
        var withFragment = original.WithFragment("frag");
        var withQuery = original.WithRawQuerystring("q=1");
        var withQueryPart = original.AddQueryPart("key", "value");

        original.Value.Should().Be("/path");
        withSegment.Should().NotBeSameAs(original);
        withFragment.Should().NotBeSameAs(original);
        withQuery.Should().NotBeSameAs(original);
        withQueryPart.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Chaining_MultipleOperations()
    {
        var result = RelativeWebUri.Create("/api")
            .AddPathSegment("users")
            .AddQueryPart("page", "1")
            .WithFragment("top");

        result.Value.Should().Be("/api/users?page=1#top");
    }
}
