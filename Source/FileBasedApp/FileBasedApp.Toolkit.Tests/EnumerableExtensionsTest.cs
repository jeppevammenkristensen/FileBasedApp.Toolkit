using System;
using System.Collections.Generic;
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

    [Fact]
    public void GetFirstRequired_WithCustomErrorMessage_ThrowsWithCustomMessage()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};
        var customMessage = "Custom no match error";

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => collection.GetFirstRequired(i => i > 10, customMessage));
        ex.Message.Should().Be(customMessage);
    }

    [Fact]
    public void GetFirstRequired_NoPredicate_ReturnsFirstElement()
    {
        // Arrange
        var collection = new List<int> {5, 2, 3};

        // Act
        var result = collection.GetFirstRequired();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetFirstOrNull_NoPredicate_ReturnsFirstElement()
    {
        // Arrange
        var collection = new List<int> {5, 2, 3};

        // Act
        var result = collection.GetFirstOrNull();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetSingleRequired_WithCustomErrorMessages_ThrowsWithCustomMessages()
    {
        // Arrange
        var collection = new List<int> {1, 2, 3};
        var noMatchMessage = "Custom no match";
        var multipleMatchMessage = "Custom multiple matches";

        // Act & Assert - no match
        var exNoMatch = Assert.Throws<InvalidOperationException>(() => 
            collection.GetSingleRequired(i => i > 10, noMatchMessage, multipleMatchMessage));
        exNoMatch.Message.Should().Be(noMatchMessage);

        // Act & Assert - multiple matches
        var exMultiple = Assert.Throws<InvalidOperationException>(() => 
            collection.GetSingleRequired(i => i > 1, noMatchMessage, multipleMatchMessage));
        exMultiple.Message.Should().Be(multipleMatchMessage);
    }

    [Fact]
    public void GetSingleOrNull_WithCustomMultipleMatchesError_ThrowsWithCustomMessage()
    {
        // Arrange
        var collection = new List<int> {1, 2, 2, 3};
        var customMessage = "Custom multiple match error";

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            collection.GetSingleOrNull(i => i == 2, customMessage));
        ex.Message.Should().Be(customMessage);
    }
}