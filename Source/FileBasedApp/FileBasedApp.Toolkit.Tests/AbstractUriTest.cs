using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>
/// Tests for <see cref="AbstractUri{TSelf}"/> methods, exercised through <see cref="RelativeWebUri"/>.
/// </summary>
[TestSubject(typeof(AbstractUri<>))]
public class AbstractUriTest
{
    [Fact]
    public void AddPathSegment_AppendsSegment()
    {
        var uri = RelativeWebUri.Create("/first");
        var result = uri.AddPathSegment(UriPathSegment.From("second"));
        result.Value.Should().Be("/first/second");
    }

    [Fact]
    public void AddPathSegment_PreservesQueryAndFragment()
    {
        var uri = RelativeWebUri.Create("/first?q=1#frag");
        var result = uri.AddPathSegment(UriPathSegment.From("second"));
        result.Value.Should().Be("/first/second?q=1#frag");
    }

    [Fact]
    public void AddPathSegment_WithTrailingSlash_DoesNotDoubleSlash()
    {
        var uri = RelativeWebUri.Create("/first/");
        var result = uri.AddPathSegment(UriPathSegment.From("second"));
        result.Value.Should().Be("/first/second");
    }

    [Theory]
    [InlineData("section1", "/path#section1")]
    [InlineData("#section1", "/path#section1")]
    public void WithFragment_AddsFragment(string fragment, string expected)
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri.WithFragment(UriFragment.From(fragment));
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void WithFragment_ReplacesExistingFragment()
    {
        var uri = RelativeWebUri.Create("/path#old");
        var result = uri.WithFragment(UriFragment.From("new"));
        result.Value.Should().Be("/path#new");
    }

    [Fact]
    public void WithFragment_PreservesQuery()
    {
        var uri = RelativeWebUri.Create("/path?q=1");
        var result = uri.WithFragment(UriFragment.From("frag"));
        result.Value.Should().Be("/path?q=1#frag");
    }

    [Theory]
    [InlineData("key=value", "/path?key=value")]
    [InlineData("?key=value", "/path?key=value")]
    public void WithRawQuerystring_AddsQuerystring(string querystring, string expected)
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri.WithRawQuerystring(UriQueryString.From(querystring));
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void WithRawQuerystring_ReplacesExistingQuery()
    {
        var uri = RelativeWebUri.Create("/path?old=1");
        var result = uri.WithRawQuerystring(UriQueryString.From("new=2"));
        result.Value.Should().Be("/path?new=2");
    }

    [Fact]
    public void WithRawQuerystring_PreservesFragment()
    {
        var uri = RelativeWebUri.Create("/path#frag");
        var result = uri.WithRawQuerystring(UriQueryString.From("q=1"));
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
    public void Operations_ReturnNewInstance()
    {
        var original = RelativeWebUri.Create("/path");
        var withSegment = original.AddPathSegment(UriPathSegment.From("new"));
        var withFragment = original.WithFragment(UriFragment.From("frag"));
        var withQuery = original.WithRawQuerystring(UriQueryString.From("q=1"));
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
            .AddPathSegment(UriPathSegment.From("users"))
            .AddQueryPart("page", "1")
            .WithFragment(UriFragment.From("top"));

        result.Value.Should().Be("/api/users?page=1#top");
    }

    [Fact]
    public void DivideOperator_WithPathSegment_AppendsSegment()
    {
        var uri = RelativeWebUri.Create("/first");
        var result = uri / UriPathSegment.From("second");
        result.Value.Should().Be("/first/second");
    }

    [Fact]
    public void DivideOperator_WithPathSegment_PreservesQueryAndFragment()
    {
        var uri = RelativeWebUri.Create("/first?q=1#frag");
        var result = uri / UriPathSegment.From("second");
        result.Value.Should().Be("/first/second?q=1#frag");
    }

    [Fact]
    public void DivideOperator_WithFragment_AddsFragment()
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri / UriFragment.From("frag");
        result.Value.Should().Be("/path#frag");
    }

    [Fact]
    public void DivideOperator_WithFragment_ReplacesExistingFragment()
    {
        var uri = RelativeWebUri.Create("/path#old");
        var result = uri / UriFragment.From("new");
        result.Value.Should().Be("/path#new");
    }

    [Fact]
    public void DivideOperator_WithQueryString_AddsQuerystring()
    {
        var uri = RelativeWebUri.Create("/path");
        var result = uri / UriQueryString.From("q=1");
        result.Value.Should().Be("/path?q=1");
    }

    [Fact]
    public void DivideOperator_WithQueryString_ReplacesExistingQuery()
    {
        var uri = RelativeWebUri.Create("/path?old=1");
        var result = uri / UriQueryString.From("new=2");
        result.Value.Should().Be("/path?new=2");
    }

    [Fact]
    public void DivideOperator_Chaining_MultipleOperatorsCombined()
    {
        var result = RelativeWebUri.Create("/api")
                     / UriPathSegment.From("users")
                     / UriQueryString.From("page=1")
                     / UriFragment.From("top");

        result.Value.Should().Be("/api/users?page=1#top");
    }

    [Fact]
    public void DivideOperator_ReturnsNewInstance()
    {
        var original = RelativeWebUri.Create("/path");
        var result = original / UriPathSegment.From("new");

        original.Value.Should().Be("/path");
        result.Should().NotBeSameAs(original);
    }

    // --- Fragment / PathSegment / QueryString properties ---

    [Fact]
    public void Fragment_WhenPresent_ReturnsValueObject()
    {
        var uri = RelativeWebUri.Create("/path#section");
        uri.Fragment.Value.Should().Be("#section");
    }

    [Fact]
    public void Fragment_WhenMissing_ReturnsEmptyValue()
    {
        var uri = RelativeWebUri.Create("/path");
        uri.Fragment.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Fragment_OnAbsoluteWebUri_ReturnsValueObject()
    {
        var uri = AbsoluteWebUri.Create("https://example.com/path#top");
        uri.Fragment.Value.Should().Be("#top");
    }

    [Fact]
    public void PathSegment_ReturnsPath()
    {
        var uri = RelativeWebUri.Create("/users");
        uri.PathSegment.Value.Should().Be("/users");
    }

    [Fact]
    public void PathSegment_OnAbsoluteWebUri_ReturnsPath()
    {
        var uri = AbsoluteWebUri.Create("https://example.com/users?q=1#frag");
        uri.PathSegment.Value.Should().Be("/users");
    }

    [Fact]
    public void QueryString_WhenPresent_ReturnsValueObject()
    {
        var uri = RelativeWebUri.Create("/path?a=1&b=2");
        uri.QueryString.Value.Should().Be("?a=1&b=2");
    }

    [Fact]
    public void QueryString_WhenMissing_ReturnsNormalizedQuestionMark()
    {
        var uri = RelativeWebUri.Create("/path");
        uri.QueryString.Value.Should().Be("");
    }

    [Fact]
    public void QueryString_OnAbsoluteWebUri_ReturnsValueObject()
    {
        var uri = AbsoluteWebUri.Create("https://example.com/path?key=value");
        uri.QueryString.Value.Should().Be("?key=value");
    }
}
