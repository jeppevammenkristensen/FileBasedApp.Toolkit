using System.Collections.Generic;
using FileBasedApp.Toolkit.CSharp;
using FluentAssertions;
using JetBrains.Annotations;
using System.IO.Abstractions.TestingHelpers;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(FileBasedAppEvaluator))]
public class FileBasedAppEvaluatorTest
{
    private static AbsolutePath TestPath(string relativePath) =>
        AbsolutePath.CurrentWorkingDirectory / relativePath;

    #region IsFileBasedApp — returns true

    [Theory]
    [InlineData("#:package FileBasedApp.Toolkit@0.16.0")]
    [InlineData("#:property PublishAot=false")]
    [InlineData("#:sdk Microsoft.NET.Sdk.Web")]
    [InlineData("#:project ./my.csproj")]
    public void IsFileBasedApp_WithFileBasedDirective_ReturnsTrue(string directive)
    {
        var content = $"""
            {directive}

            Console.WriteLine("hello");
            """;

        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeTrue();
    }

    [Fact]
    public void IsFileBasedApp_WithMultipleFileBasedDirectives_ReturnsTrue()
    {
        const string content = """
            #:package FileBasedApp.Toolkit@0.16.0
            #:package Microsoft.CodeAnalysis.CSharp@*
            #:property PublishAot=false

            Console.WriteLine("hello");
            """;

        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeTrue();
    }

    #endregion

    #region IsFileBasedApp — returns false

    [Fact]
    public void IsFileBasedApp_WithNoDirectives_ReturnsFalse()
    {
        const string content = """
            Console.WriteLine("hello");
            """;

        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeFalse();
    }

    [Fact]
    public void IsFileBasedApp_WithClassDeclarationAndDirective_ReturnsFalse()
    {
        // A file with a class (not a top-level script) should not be treated as a file-based app
        // even if it happens to contain a matching comment in leading trivia
        const string content = """
            // #:package Something@1.0.0
            namespace MyApp;

            public class Foo { }
            """;

        var path = TestPath("Foo.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeFalse();
    }

    [Fact]
    public void IsFileBasedApp_WithDirectiveInBodyNotLeadingTrivia_ReturnsFalse()
    {
        // Directive appears after real code — not in leading trivia of the compilation unit
        const string content = """
            Console.WriteLine("hello");
            #:package SomePkg@1.0.0
            """;

        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeFalse();
    }

    [Fact]
    public void IsFileBasedApp_WithUnknownDirectivePrefix_ReturnsFalse()
    {
        // e.g. #:reference is not a recognised file-based directive
        const string content = """
            #:reference Something
            Console.WriteLine("hello");
            """;

        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeFalse();
    }

    [Fact]
    public void IsFileBasedApp_EmptyFile_ReturnsFalse()
    {
        var path = TestPath("empty.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(string.Empty)
        });

        var sut = new FileBasedAppEvaluator(fileSystem);

        sut.IsFileBasedApp(path).Should().BeFalse();
    }

    #endregion
}
