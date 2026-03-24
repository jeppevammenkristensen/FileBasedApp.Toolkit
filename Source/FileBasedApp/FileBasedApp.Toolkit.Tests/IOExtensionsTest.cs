using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(IO))]
public class IOExtensionsTest
{
    [Fact]

    public void GetAncestors_ShouldGenerateExpectedResult()
    {
        // check is runtime is windows
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            var path = AbsolutePath.Create(@"C:\Users\user\Documents\SomeOther\test.txt");
            var ancestors = path.GetAncestors(true).ToList();
            ancestors.Should().HaveCount(6);
            ancestors[0].FileName.Should().Be("test.txt");
            ancestors[1].FileName.Should().Be("SomeOther");
            ancestors[2].FileName.Should().Be("Documents");
            ancestors[3].FileName.Should().Be("user");
            ancestors[4].FileName.Should().Be("Users");
            ancestors[5].Value.Should().Be("C:\\");
        }
        else
        {
            // var path = AbsolutePath.Create("/home/user/Documents/SomeOther/test.txt");
            // var ancestors = path.GetAncestors(true).ToList();
            // ancestors.Should().HaveCount(5);
            // ancestors[0].FileName.Should().Be("test.txt");
            // ancestors[1].FileName.Should().Be("SomeOther");
            // ancestors[2].FileName.Should().Be("Documents");
            // ancestors[3].FileName.Should().Be("user");
            // ancestors[4].Value.Should().Be("/");
        }
    }
    /// <summary>
    /// This is low practical way to ensure that the created path works on both Windows and Linux.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    private static AbsolutePath TestPath(string relativePath) =>
        AbsolutePath.CurrentWorkingDirectory / relativePath;

    [Fact]
    public void FindInFiles_WithStreamPredicate_ReturnsMatchingFiles()
    {
        var matchPath = TestPath("match.txt");
        var noMatchPath = TestPath("nomatch.txt");
        var alsoMatchPath = TestPath("also-match.txt");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { matchPath.Value, new MockFileData("hello world") },
            { noMatchPath.Value, new MockFileData("goodbye") },
            { alsoMatchPath.Value, new MockFileData("hello there") },
        });

        var files = new[] { matchPath, noMatchPath, alsoMatchPath };

        var result = files.FindInFiles(stream =>
        {
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd().Contains("hello");
        }, fs).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(matchPath);
        result.Should().Contain(alsoMatchPath);
    }

    [Fact]
    public void FindInFiles_WithStreamPredicate_NoMatches_ReturnsEmpty()
    {
        var aPath = TestPath("a.txt");
        var bPath = TestPath("b.txt");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { aPath.Value, new MockFileData("foo") },
            { bPath.Value, new MockFileData("bar") },
        });

        var result = new[] { aPath, bPath }.FindInFiles(_ => false, fs).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void FindInFiles_WithStreamPredicate_EmptySource_ReturnsEmpty()
    {
        var result = Array.Empty<AbsolutePath>().FindInFiles(_ => true).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void FindInFiles_WithRegex_ByLine_ReturnsMatchingFiles()
    {
        var appPath = TestPath("app.cs");
        var libPath = TestPath("lib.cs");
        var scriptPath = TestPath("script.cs");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { appPath.Value, new MockFileData("#:package FileBasedApp.Toolkit@0.16.0\nusing System;") },
            { libPath.Value, new MockFileData("using System;\nclass Foo {}") },
            { scriptPath.Value, new MockFileData("#:package SomeOther@1.0\n#:property PublishAot=false") },
        });

        var files = new[] { appPath, libPath, scriptPath };

        var regex = new Regex(@"^#:package");
        var result = files.FindInFiles(regex, IO.FileSearchStrategy.ByLine, fs).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(appPath);
        result.Should().Contain(scriptPath);
    }

    [Fact]
    public void FindInFiles_WithRegex_ByLine_NoMatches_ReturnsEmpty()
    {
        var filePath = TestPath("a.txt");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath.Value, new MockFileData("no match here\nstill nothing") },
        });

        var result = new[] { filePath }.FindInFiles(new Regex(@"^#:package"), IO.FileSearchStrategy.ByLine, fs).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void FindInFiles_WithRegex_AllText_MatchesAcrossLines()
    {
        var multiPath = TestPath("multi.txt");
        var singlePath = TestPath("single.txt");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { multiPath.Value, new MockFileData("start\nmiddle\nend") },
            { singlePath.Value, new MockFileData("no spanning here") },
        });

        var files = new[] { multiPath, singlePath };

        // Regex that spans across lines — only matches with AllText strategy
        var regex = new Regex(@"start.*end", RegexOptions.Singleline);
        var result = files.FindInFiles(regex, IO.FileSearchStrategy.AllText, fs).ToList();

        result.Should().ContainSingle()
            .Which.Should().Be(multiPath);
    }

    [Fact]
    public void FindInFiles_WithRegex_ByLine_DoesNotMatchAcrossLines()
    {
        var filePath = TestPath("multi.txt");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath.Value, new MockFileData("start\nmiddle\nend") },
        });

        // Same pattern but ByLine — should NOT match since "start" and "end" are on different lines
        var regex = new Regex(@"start.*end");
        var result = new[] { filePath }.FindInFiles(regex, IO.FileSearchStrategy.ByLine, fs).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void SafeDeleteDirectory_HandleNonExistentDirectory()
    {
        var nonExistentPath = TestPath("nonexistent");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
        });

        nonExistentPath.SafeDeleteDirectory(fileSystem:fs);
    }

    [Fact]
    public void SafeDeleteDirectory_NonExistentDirectory_DoesNotInvokeHandler()
    {
        var dirPath = TestPath("nonexistent2");

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
        });

        // The directory doesn't exist, so the guard prevents deletion and no exception is raised
        Exception? captured = null;
        dirPath.SafeDeleteDirectory(exceptionHandler: ex => captured = ex, fileSystem: fs);

        captured.Should().BeNull();
    }

    [Fact]
    public void SafeDeleteDirectory_PassesExceptionToHandler()
    {
        var dirPath = TestPath("failing");

        var fs = Substitute.For<IFileSystem>();
        fs.Directory.Exists(dirPath.Value).Returns(true);
        fs.Directory.When(d => d.Delete(dirPath.Value, true)).Throw(new IOException("Directory is in use"));

        Exception? captured = null;
        dirPath.SafeDeleteDirectory(exceptionHandler: ex => captured = ex, fileSystem: fs);

        captured.Should().NotBeNull();
        captured.Should().BeOfType<IOException>();
        captured!.Message.Should().Be("Directory is in use");
    }

    [Fact]
    public void SafeDeleteDirectory_DeletesExistingDirectory()
    {
        var dirPath = TestPath("toDelete");
        var filePath = dirPath / "file.txt";

        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath.Value, new MockFileData("content") },
        });

        fs.Directory.Exists(dirPath.Value).Should().BeTrue();

        dirPath.SafeDeleteDirectory(fileSystem: fs);

        fs.Directory.Exists(dirPath.Value).Should().BeFalse();
    }
}