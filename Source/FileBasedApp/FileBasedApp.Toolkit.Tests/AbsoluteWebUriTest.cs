using System;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(AbsoluteWebUri))]
public class AbsoluteWebUriTest
{
    // --- AbstractUri divide operators exercised through AbsoluteWebUri ---

    [Fact]
    public void DivideOperator_WithPathSegment_AppendsSegment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/first");
        var result = uri / UriPathSegment.From("second");
        result.Value.Should().Be("https://dr.dk/first/second");
    }

    [Fact]
    public void DivideOperator_WithPathSegment_PreservesQueryAndFragment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/first?q=1#frag");
        var result = uri / UriPathSegment.From("second");
        result.Value.Should().Be("https://dr.dk/first/second?q=1#frag");
    }

    [Fact]
    public void AddPathSegment_WithTrailingSlash_DoesNotDoubleSlash()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/first/");
        var result = uri.AddPathSegment(UriPathSegment.From("second"));
        result.Value.Should().Be("https://dr.dk/first/second");
    }

    [Fact]
    public void DivideOperator_WithFragment_AddsFragment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path");
        var result = uri / UriFragment.From("frag");
        result.Value.Should().Be("https://dr.dk/path#frag");
    }

    [Fact]
    public void DivideOperator_WithFragment_ReplacesExistingFragment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path#old");
        var result = uri / UriFragment.From("new");
        result.Value.Should().Be("https://dr.dk/path#new");
    }

    [Fact]
    public void DivideOperator_WithQueryString_AddsQuerystring()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path");
        var result = uri / UriQueryString.From("q=1");
        result.Value.Should().Be("https://dr.dk/path?q=1");
    }

    [Fact]
    public void DivideOperator_WithQueryString_ReplacesExistingQuery()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path?old=1");
        var result = uri / UriQueryString.From("new=2");
        result.Value.Should().Be("https://dr.dk/path?new=2");
    }

    [Fact]
    public void AddQueryPart_AddsToEmptyQuery()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path");
        var result = uri.AddQueryPart("key", "value");
        result.Value.Should().Be("https://dr.dk/path?key=value");
    }

    [Fact]
    public void AddQueryPart_AppendsToExistingQuery()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path?existing=1");
        var result = uri.AddQueryPart("key", "value");
        result.Value.Should().Contain("existing=1");
        result.Value.Should().Contain("key=value");
    }

    [Fact]
    public void HasQuery_ReflectsPresence()
    {
        AbsoluteWebUri.Create("https://dr.dk/path?q=1").HasQuery.Should().BeTrue();
        AbsoluteWebUri.Create("https://dr.dk/path").HasQuery.Should().BeFalse();
    }

    [Fact]
    public void HasFragments_ReflectsPresence()
    {
        AbsoluteWebUri.Create("https://dr.dk/path#frag").HasFragments.Should().BeTrue();
        AbsoluteWebUri.Create("https://dr.dk/path").HasFragments.Should().BeFalse();
    }

    [Fact]
    public void DivideOperator_Chaining_BuildsFullUri()
    {
        var result = AbsoluteWebUri.Create("https://dr.dk/api")
                     / UriPathSegment.From("users")
                     / UriQueryString.From("page=1")
                     / UriFragment.From("top");

        result.Value.Should().Be("https://dr.dk/api/users?page=1#top");
    }

    [Fact]
    public void DivideOperator_ReturnsNewInstance()
    {
        var original = AbsoluteWebUri.Create("https://dr.dk/path");
        var result = original / UriPathSegment.From("new");

        original.Value.Should().Be("https://dr.dk/path");
        result.Should().NotBeSameAs(original);
    }

    [Fact]
    public void DivideOperator_WithRelativeWebUri_CombinesPaths()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/base/");
        var relative = RelativeWebUri.Create("child/path");
        var result = uri / relative;
        result.Value.Should().Be("https://dr.dk/base/child/path");
    }

    [Fact]
    public void WithRelativeUri_CombinesPaths()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/base/");
        var relative = RelativeWebUri.Create("child");
        var result = uri.WithRelativeUri(relative);
        result.Value.Should().Be("https://dr.dk/base/child");
    }

    [Fact]
    public void ValueWithoutTrailingSeparator_RemovesTrailingSlash()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path/");
        uri.ValueWithoutTrailingSeparator.Should().Be("https://dr.dk/path");
    }

    [Fact]
    public void ValueWithoutTrailingSeparator_LeavesNoTrailingSlash()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path");
        uri.ValueWithoutTrailingSeparator.Should().Be("https://dr.dk/path");
    }

    [Fact]
    public void ValueWithTrailingSeparator_AddsTrailingSlash()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path");
        uri.ValueWithTrailingSeparator.Should().Be("https://dr.dk/path/");
    }

    [Fact]
    public void ValueWithTrailingSeparator_EnsuresSingleTrailingSlash()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/path/");
        uri.ValueWithTrailingSeparator.Should().Be("https://dr.dk/path/");
    }

    // --- Create / Parse / TryParse ---

    [Fact]
    public void Create_ValidAbsoluteUri_ReturnsInstance()
    {
        var result = AbsoluteWebUri.Create("https://dr.dk/path");
        result.Value.Should().Be("https://dr.dk/path");
    }

    [Fact]
    public void Create_RelativeUri_Throws()
    {
        var act = () => AbsoluteWebUri.Create("/just/a/path");
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void TryParse_ValidAbsoluteUri_ReturnsTrueAndInstance()
    {
        var success = AbsoluteWebUri.TryParse("https://dr.dk/path", null, out var result);
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Value.Should().Be("https://dr.dk/path");
    }

    [Fact]
    public void TryParse_RelativeUri_ReturnsFalse()
    {
        var success = AbsoluteWebUri.TryParse("/just/a/path", null, out var result);
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void ImplicitConversion_ToUri_ReturnsUnderlyingUri()
    {
        var absolute = AbsoluteWebUri.Create("https://dr.dk/path");
        Uri uri = absolute;
        uri.Should().Be(absolute.Uri);
    }

    // --- Parent ---

    [Fact]
    public void Parent_DropsLastPathSegment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/parent/child");
        var result = uri.Parent();
        result.Value.Should().Be("https://dr.dk/parent");
    }

    [Fact]
    public void Parent_PreservesQueryAndFragment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/parent/child?seg=1#frag");
        var result = uri.Parent();
        result.Value.Should().Be("https://dr.dk/parent?seg=1#frag");
    }

    [Fact]
    public void Parent_NoSegments_ThrowsAnException()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk");
        Assert.Throws<InvalidOperationException>(uri.Parent);
    }
    
    
    [Fact]
    public void Parent_FromSingleSegment_ReturnsRoot()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/only");
        var result = uri.Parent();
        result.Value.Should().Be("https://dr.dk/");
    }

    [Fact]
    public void Parent_ReturnsNewInstance()
    {
        var original = AbsoluteWebUri.Create("https://dr.dk/parent/child");
        var result = original.Parent();
        result.Should().NotBeSameAs(original);
        original.Value.Should().Be("https://dr.dk/parent/child");
    }

    [Fact]
    public void Parent_ChainedTwice_DropsTwoSegments()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/a/b/c");
        var result = uri.Parent().Parent();
        result.Value.Should().Be("https://dr.dk/a");
    }

    // --- WithPathSegment ---


    [Fact]
    public void WithPathSegment_ReplacesExistingPath()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/1/2");
        var result = uri.WithPathSegment(UriPathSegment.From("3"));
        result.Value.Should().Be("https://dr.dk/3");
    }

    [Fact]
    public void WithPathSegment_OnRootPath_SetsPath()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/");
        var result = uri.WithPathSegment(UriPathSegment.From("new"));
        result.Value.Should().Be("https://dr.dk/new");
    }

    [Fact]
    public void WithPathSegment_PreservesQueryAndFragment()
    {
        var uri = AbsoluteWebUri.Create("https://dr.dk/old/path?q=1#frag");
        var result = uri.WithPathSegment(UriPathSegment.From("new"));
        result.Value.Should().Be("https://dr.dk/new?q=1#frag");
    }

    [Fact]
    public void WithPathSegment_ReturnsNewInstance()
    {
        var original = AbsoluteWebUri.Create("https://dr.dk/a/b");
        var result = original.WithPathSegment(UriPathSegment.From("c"));
        result.Should().NotBeSameAs(original);
        original.Value.Should().Be("https://dr.dk/a/b");
    }
}
