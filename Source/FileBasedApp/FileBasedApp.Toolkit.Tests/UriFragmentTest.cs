using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(UriFragment))]
public class UriFragmentTest
{
    [Theory]
    [InlineData("section", "#section")]
    [InlineData("#section", "#section")]
    [InlineData("##double", "#double")]
    public void From_NormalizesLeadingHash(string input, string expected)
    {
        var fragment = UriFragment.From(input);
        fragment.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("section", "section")]
    [InlineData("#section", "section")]
    [InlineData("##double", "double")]
    public void ValueWithHashtag_ReturnsFragmentWithoutHashtag(string input, string expected)
    {
        var fragment = UriFragment.From(input);
        fragment.ValueWithHashtag.Should().Be(expected);
    }
}