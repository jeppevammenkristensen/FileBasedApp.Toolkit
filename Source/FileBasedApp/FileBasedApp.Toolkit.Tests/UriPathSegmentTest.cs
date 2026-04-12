using FluentAssertions;
using JetBrains.Annotations;
using Vogen;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(UriPathSegment))]
public class UriPathSegmentTest
{
    [Theory]
    [InlineData("users", "users")]
    [InlineData("users/", "users")]
    [InlineData("/users", "/users")]
    [InlineData("item-1", "item-1")]
    [InlineData("item_1.ext", "item_1.ext")]
    public void From_ValidSegment_NormalizesAndStores(string input, string expected)
    {
        var segment = UriPathSegment.From(input);
        segment.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("has space")]
    [InlineData("has?query")]
    [InlineData("has#fragment")]
    public void From_InvalidSegment_Throws(string input)
    {
        var act = () => UriPathSegment.From(input);
        act.Should().Throw<ValueObjectValidationException>();
    }

    [Fact]
    public void WithLeadingSeparator_PrependsSlash()
    {
        var segment = UriPathSegment.From("users");
        segment.WithLeadingSeparator().Should().Be("/users");
    }

    [Fact]
    public void WithTrailingSeparator_AppendsSlash()
    {
        var segment = UriPathSegment.From("users");
        segment.WithTrailingSeparator().Should().Be("users/");
    }

    [Fact]
    public void WithLeadingAndTrailingSeparator_WrapsInSlashes()
    {
        var segment = UriPathSegment.From("users");
        segment.WithLeadingAndTrailingSeparator().Should().Be("/users/");
    }
}