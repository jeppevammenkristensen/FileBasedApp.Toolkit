using System;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(StringExtensions))]
public class StringExtensionsTest
{

    [Theory]
    [InlineData(null, StringNullCheck.Null, true)]
    [InlineData("", StringNullCheck.Null, false)]
    [InlineData("  ", StringNullCheck.Null, false)]
    [InlineData("hello", StringNullCheck.Null, false)]
    [InlineData(null, StringNullCheck.NullOrEmpty, true)]
    [InlineData("", StringNullCheck.NullOrEmpty, true)]
    [InlineData("  ", StringNullCheck.NullOrEmpty, false)]
    [InlineData("hello", StringNullCheck.NullOrEmpty, false)]
    [InlineData(null, StringNullCheck.NullOrWhitespace, true)]
    [InlineData("", StringNullCheck.NullOrWhitespace, true)]
    [InlineData("  ", StringNullCheck.NullOrWhitespace, true)]
    [InlineData("hello", StringNullCheck.NullOrWhitespace, false)]
    public void NullCheck_ReturnsExpectedResult(string? value, StringNullCheck check, bool expected)
    {
        Assert.Equal(expected, value.NullCheck(check));
    }

    // --- Invalid enum value ---

    [Fact]
    public void NullCheck_InvalidEnum_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            "test".NullCheck((StringNullCheck)999));
    }
}