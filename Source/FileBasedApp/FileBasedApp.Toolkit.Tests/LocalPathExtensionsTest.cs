using System;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>Verifies lexical traversal and home-directory expansion without accessing the file system.</summary>
[TestSubject(typeof(LocalPathExtensions))]
public class LocalPathExtensionsTest
{
    [Theory]
    [InlineData("~", true)]
    [InlineData("~/", true)]
    [InlineData("~/Documents", false)]
    [InlineData("~someone", false)]
    [InlineData("folder/~", false)]
    [InlineData("", false)]
    [InlineData("..", false)]
    public void IsHome_RecognizesOnlyTheHomeMarker(string value, bool expected)
    {
        new LocalPath(value).IsHome.Should().Be(expected);
    }

    [Theory]
    [InlineData("~", true)]
    [InlineData("~/Documents", true)]
    [InlineData("~/Documents/Projects/file.txt", true)]
    [InlineData("~someone/Documents", false)]
    [InlineData("folder/~/file.txt", false)]
    [InlineData("Documents/file.txt", false)]
    [InlineData("", false)]
    [InlineData(".", false)]
    [InlineData("..", false)]
    [InlineData("../../file.txt", false)]
    public void CanBeTransformedToAbsolute_RecognizesLeadingHomeMarker(string value, bool expected)
    {
        new LocalPath(value).CanBeTransformedToAbsolute().Should().Be(expected);
    }

    [Theory]
    [InlineData("~", "")]
    [InlineData("~/Documents", "Documents")]
    [InlineData("~/Documents/Projects/file.txt", "Documents/Projects/file.txt")]
    public void TransformAbsolute_ExpandsHomeAndPreservesEachSegment(string value, string suffix)
    {
        var home = Environment.SpecialFolder.UserProfile.GetSpecialFolder();
        var expected = suffix.Length == 0 ? home : home / suffix;

        var result = new LocalPath(value).TransformAbsolute();

        result.Should().Be((LocalPath) expected);
        result.IsAbsolute.Should().BeTrue();
    }

    [Theory]
    [InlineData("Documents/file.txt")]
    [InlineData("~someone/Documents")]
    [InlineData("folder/~/file.txt")]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../../file.txt")]
    public void TransformAbsolute_LeavesNonHomeRelativePathsUnchanged(string value)
    {
        var path = new LocalPath(value);

        path.TransformAbsolute().Should().Be(path);
    }

    [Fact]
    public void HomeHelpers_LeaveAbsolutePathsContainingHomeMarkerUnchanged()
    {
        LocalPath path = AbsolutePath.CurrentWorkingDirectory / "~" / "file.txt";

        path.IsHome.Should().BeFalse();
        path.CanBeTransformedToAbsolute().Should().BeFalse();
        path.TransformAbsolute().Should().Be(path);
    }

    [Theory]
    [InlineData("foo/bar/baz.txt", new[] {"foo", "bar", "baz.txt"})]
    [InlineData("~/Documents/Projects/file.txt", new[] {"~", "Documents", "Projects", "file.txt"})]
    [InlineData("foo", new[] {"foo"})]
    [InlineData("~", new[] {"~"})]
    [InlineData("..", new[] {".."})]
    [InlineData("../../file.txt", new[] {"..", "..", "file.txt"})]
    [InlineData("", new string[0])]
    [InlineData(".", new string[0])]
    public void Traversal_ReturnsIndividualSegmentsInBothOrders(string value, string[] expected)
    {
        var path = new LocalPath(value);

        path.GetSegments().Select(segment => segment.Value).Should().Equal(expected);
        path.Traverse().Select(segment => segment.Value).Should().Equal(expected.Reverse());
    }

    [Fact]
    public void Traversal_PreservesAbsoluteRootAndAllowsReconstruction()
    {
        LocalPath path = AbsolutePath.CurrentWorkingDirectory / "folder" / "file.txt";
        var segments = path.GetSegments().ToArray();

        segments[0].IsAbsolute.Should().BeTrue();
        segments[0].Parent.Should().BeNull();
        segments.Skip(1).Should().OnlyContain(segment => !segment.IsAbsolute);
        segments[^2].Should().Be(new LocalPath("folder"));
        segments[^1].Should().Be(new LocalPath("file.txt"));
        segments.Aggregate((left, right) => left / right).Should().Be(path);
        path.Traverse().Should().Equal(segments.Reverse());
    }

    [Fact]
    public void Traversal_AbsoluteRootIsASingleSegment()
    {
        LocalPath root = AbsolutePath.CurrentWorkingDirectory;
        while (root.Parent is { } parent)
        {
            root = parent;
        }

        root.GetSegments().Should().Equal(root);
        root.Traverse().Should().Equal(root);
        root.CanBeTransformedToAbsolute().Should().BeFalse();
        root.TransformAbsolute().Should().Be(root);
    }
}