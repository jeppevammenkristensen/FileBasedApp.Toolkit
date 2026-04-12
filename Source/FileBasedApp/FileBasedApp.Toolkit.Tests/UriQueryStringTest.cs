using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(UriQueryString))]
public class UriQueryStringTest
{
    [Theory]
    [InlineData("key=value", "?key=value")]
    [InlineData("?key=value", "?key=value")]
    [InlineData("??doubled", "?doubled")]
    [InlineData("a=1&b=2", "?a=1&b=2")]
    public void From_NormalizesLeadingQuestionMark(string input, string expected)
    {
        var query = UriQueryString.From(input);
        query.Value.Should().Be(expected);
    }
}