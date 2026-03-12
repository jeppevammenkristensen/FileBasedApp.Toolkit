using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;


public class EnumerableExtensionsTest
{
    [Fact]
    public void GetFirstRequired_ThrowsExceptionWhenNoMatch()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetFirstRequired(i => i > 5));
        ex.Message.Should().Be("No item found");
    }

    [Fact]
    public void GetFirstRequired_ReturnsFirstMatchingElement()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetFirstRequired(i => i > 1);

        // Assert
        result.Should().Be(2);
        
    }

    [Fact]
    public void GetFirstOrNull_ReturnsNullWhenNoMatch()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetFirstOrNull(i => i > 5);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFirstOrNull_ReturnsFirstMatchingElement()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetFirstOrNull(i => i > 1);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetSingleRequired_ThrowsExceptionWhenNoMatch()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetSingleRequired(i => i > 5));
        ex.Message.Should().Be("No item found");
    }

    [Fact]
    public void GetSingleRequired_ThrowsExceptionWhenMultipleMatches()
    {
        // Arrange
        var collection = new List<int> {1, 2, 2, 3};

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetSingleRequired(i => i == 2));
        ex.Message.Should().Be("Found more than 1 matches");
    }

    [Fact]
    public void GetSingleRequired_ReturnsSingleMatchingElement()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetSingleRequired(i => i == 2);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetSingleOrNull_ReturnsNullWhenNoMatch()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetSingleOrNull(i => i > 5);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSingleOrNull_ThrowsExceptionWhenMultipleMatches()
    {
        // Arrange
        var collection = new List<int> {1, 2, 2, 3};

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetSingleOrNull(i => i == 2));
        ex.Message.Should().Be("Found more than 1 matches");
    }

    [Fact]
    public void GetSingleOrNull_ReturnsSingleMatchingElement()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};

        // Act
        var result = collection.GetSingleOrNull(i => i == 2);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetSingleOrNull_NullableSource_ReturnsNullWhenNoMatch()
    {
        // Arrange
        var collection = new List<int?> {1, 2, 3};

        // Act
        var result = collection.GetSingleOrNull(i => i > 5);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSingleOrNull_NullableSource_ThrowsExceptionWhenMultipleMatches()
    {
        // Arrange
        var collection = new List<int?> {1, 2, 2, 3};

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetSingleOrNull(i => i == 2));
        ex.Should().NotBeNull();
    }

    [Fact]
    public void GetSingleOrNull_NullableSource_ReturnsSingleMatchingElement()
    {
        // Arrange
        var collection = new List<int?> {1, 2, 3};

        // Act
        var result = collection.GetSingleOrNull(i => i == 2);

        // Assert
        result.Should().Be(2);
    }
}