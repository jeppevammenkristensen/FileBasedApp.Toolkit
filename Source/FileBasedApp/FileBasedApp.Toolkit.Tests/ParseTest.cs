using System;
using System.Globalization;
using FileBasedApp.Toolkit;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(ParseExtensions))]
public class ParseExtensionsTest
{

    [Theory]
    [InlineData("Incorrect")]
    [InlineData("")]
    [InlineData(null)]
    public void SafeParse_InvalidStruct_ReturnsNullableStruct(string? input)
    {
        var correct = input.TryParseTo(out int result);
        correct.Should().BeFalse();
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("Incorrect")]
    [InlineData("")]
    [InlineData(null)]
    public void RequiredParse_InvalidStruct_ReturnsNullableStruct(string? input)
    {
        Assert.Throws<InvalidOperationException>(() => input.RequiredParse<int>());
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-1", -1)]
    [InlineData("0", 0)]
    [InlineData("2147483647", int.MaxValue)]
    public void TryParseTo_ValidInt_ReturnsTrueAndParsedValue(string input, int expected)
    {
        var success = input.TryParseTo(out int result);
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    public void TryParseTo_ValidBool_ReturnsTrueAndParsedValue(string input, bool expected)
    {
        var success = input.TryParseTo(out bool result);
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("3.14", 3.14)]
    [InlineData("-0.5", -0.5)]
    public void TryParseTo_ValidDouble_ReturnsTrueAndParsedValue(string input, double expected)
    {
        var success = input.TryParseTo(CultureInfo.InvariantCulture, out double result);
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("100", 100)]
    public void RequiredParse_ValidInt_ReturnsParsedValue(string input, int expected)
    {
        var result = input.RequiredParse<int>();
        result.Should().Be(expected);
    }

    [Fact]
    public void TryParseTo_WithFormatProvider_ParsesDecimalWithComma()
    {
        var danish = new CultureInfo("da-DK");
        var success = "1.234,56".TryParseTo(danish, out decimal result);
        success.Should().BeTrue();
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void TryParseTo_WithFormatProvider_ParsesDecimalWithDot()
    {
        var success = "1,234.56".TryParseTo(CultureInfo.InvariantCulture, out decimal result);
        success.Should().BeTrue();
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void TryParseTo_WithFormatProvider_WrongCultureReturnsFalse()
    {
        var danish = new CultureInfo("da-DK");
        // "1,234.56" is not valid in da-DK where comma is decimal separator
        var success = "1,234.56".TryParseTo(danish, out decimal _);
        success.Should().BeFalse();
    }

    [Fact]
    public void RequiredParse_WithFormatProvider_ParsesCorrectly()
    {
        var danish = new CultureInfo("da-DK");
        var result = "1.234,56".RequiredParse<decimal>(danish);
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void RequiredParse_WithFormatProvider_WrongCultureThrows()
    {
        var danish = new CultureInfo("da-DK");
        var act = () => "1,234.56".RequiredParse<decimal>(danish);
        act.Should().Throw<InvalidOperationException>();
    }

    // --- SafeParseToStruct ---

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-1", -1)]
    [InlineData("0", 0)]
    public void SafeParseToStruct_ValidInt_ReturnsValue(string input, int expected)
    {
        var result = ParseExtensions.SafeParseToStruct<int>(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void SafeParseToStruct_InvalidInt_ReturnsNull(string? input)
    {
        var result = ParseExtensions.SafeParseToStruct<int>(input);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void SafeParseToStruct_ValidBool_ReturnsValue(string input, bool expected)
    {
        var result = ParseExtensions.SafeParseToStruct<bool>(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void SafeParseToStruct_WithFormatProvider_ParsesCorrectly()
    {
        var danish = new CultureInfo("da-DK");
        var result = ParseExtensions.SafeParseToStruct<decimal>("1.234,56", danish);
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void SafeParseToStruct_WithFormatProvider_WrongCultureReturnsNull()
    {
        var danish = new CultureInfo("da-DK");
        var result = ParseExtensions.SafeParseToStruct<decimal>("1,234.56", danish);
        result.Should().BeNull();
    }

    // --- SafeParseToClass ---

    [Fact]
    public void SafeParseToClass_ValidInput_ReturnsValue()
    {
        var result = ParseExtensions.SafeParseToClass<ParsableTestClass>("valid:hello");
        result.Should().NotBeNull();
        result!.Value.Should().Be("hello");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void SafeParseToClass_InvalidInput_ReturnsNull(string? input)
    {
        var result = ParseExtensions.SafeParseToClass<ParsableTestClass>(input);
        result.Should().BeNull();
    }

    [Fact]
    public void SafeParseToClass_WithFormatProvider_PassesProvider()
    {
        var result = ParseExtensions.SafeParseToClass<ParsableTestClass>("valid:world", CultureInfo.InvariantCulture);
        result.Should().NotBeNull();
        result!.Value.Should().Be("world");
    }

    /// <summary>
    /// A minimal IParsable class for testing SafeParseToClass.
    /// Parses strings in the format "valid:{value}", returns null otherwise.
    /// </summary>
    private class ParsableTestClass : IParsable<ParsableTestClass>
    {
        public string Value { get; private init; } = "";

        public static ParsableTestClass Parse(string s, IFormatProvider? provider)
        {
            if (TryParse(s, provider, out var result))
                return result;
            throw new FormatException($"Cannot parse '{s}'");
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out ParsableTestClass result)
        {
            if (s is not null && s.StartsWith("valid:"))
            {
                result = new ParsableTestClass { Value = s["valid:".Length..] };
                return true;
            }

            result = default!;
            return false;
        }
    }
}