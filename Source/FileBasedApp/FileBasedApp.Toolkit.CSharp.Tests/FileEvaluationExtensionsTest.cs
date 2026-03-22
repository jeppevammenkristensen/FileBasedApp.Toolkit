using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(FileEvaluationExtensions))]
public class FileEvaluationExtensionsTest
{
    private static AbsolutePath TestPath(string relativePath) =>
        AbsolutePath.CurrentWorkingDirectory / relativePath;

    #region LoadCsharpFile — valid input

    [Fact]
    public void LoadCsharpFile_ValidCode_Strict_ReturnsSyntaxTree()
    {
        var path = TestPath("valid.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new("Console.WriteLine(\"Hello\");")
        });

        var result = path.LoadCsharpFile(strict: true, fileSystem);

        result.Should().NotBeNull();
        result.ContainsDiagnostics.Should().BeFalse();
    }

    [Fact]
    public void LoadCsharpFile_ValidCode_NonStrict_ReturnsSyntaxTree()
    {
        var path = TestPath("valid.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new("Console.WriteLine(\"Hello\");")
        });

        var result = path.LoadCsharpFile(strict: false, fileSystem);

        result.Should().NotBeNull();
        result.ContainsDiagnostics.Should().BeFalse();
    }

    #endregion

    #region LoadCsharpFile — invalid input

    [Fact]
    public void LoadCsharpFile_InvalidCode_Strict_ThrowsInvalidOperationException()
    {
        var path = TestPath("invalid.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new("class {")
        });

        var act = () => path.LoadCsharpFile(strict: true, fileSystem);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LoadCsharpFile_InvalidCode_NonStrict_ReturnsSyntaxTreeWithDiagnostics()
    {
        var path = TestPath("invalid.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new("class {")
        });

        var result = path.LoadCsharpFile(strict: false, fileSystem);

        result.Should().NotBeNull();
        result.ContainsDiagnostics.Should().BeTrue();
    }

    #endregion
}