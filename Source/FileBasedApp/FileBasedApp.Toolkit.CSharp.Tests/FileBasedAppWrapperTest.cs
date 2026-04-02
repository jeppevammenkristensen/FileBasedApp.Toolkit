using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FileBasedApp.Toolkit.CSharp;
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(FileBasedAppWrapper))]
public class FileBasedAppWrapperTest
{
    private static AbsolutePath TestPath(string relativePath) =>
        AbsolutePath.CurrentWorkingDirectory / relativePath;

    private static FileBasedAppWrapper CreateWrapper(string content)
    {
        var path = TestPath("app.cs");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [path.Value] = new(content)
        });
        return new FileBasedAppWrapper(path, fileSystem);
    }

    [Fact]
    public void PackageDirectives_SinglePackage_ReturnsSingleDirective()
    {
        var wrapper = CreateWrapper("""
            #:package My.Package@1.0.0
            Console.WriteLine("Hello");
            """);

        var packages = wrapper.PackageDirectives.ToList();

        packages.Should().HaveCount(1);
        packages[0].PackageInfo.Should().NotBeNull();
        packages[0].PackageInfo!.Name.Should().Be("My.Package");
        packages[0].PackageInfo!.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void PackageDirectives_MultiplePackages_ReturnsAllDirectives()
    {
        var wrapper = CreateWrapper("""
            #:package First.Package@1.0.0
            #:package Second.Package@2.0.0
            #:package Third.Package@3.0.0
            Console.WriteLine("Hello");
            """);

        var packages = wrapper.PackageDirectives.ToList();

        packages.Should().HaveCount(3);
        packages[0].PackageInfo!.Name.Should().Be("First.Package");
        packages[0].PackageInfo!.Version.Should().Be("1.0.0");
        packages[1].PackageInfo!.Name.Should().Be("Second.Package");
        packages[1].PackageInfo!.Version.Should().Be("2.0.0");
        packages[2].PackageInfo!.Name.Should().Be("Third.Package");
        packages[2].PackageInfo!.Version.Should().Be("3.0.0");
    }

    [Fact]
    public void PackageDirectives_MixedDirectives_ReturnsOnlyPackageDirectives()
    {
        var wrapper = CreateWrapper("""
            #:package My.Package@1.0.0
            #:property PublishAot=false
            #:sdk Microsoft.NET.Sdk.Web
            #:package Another.Package@2.0.0
            #:project ./my.csproj
            Console.WriteLine("Hello");
            """);

        var packages = wrapper.PackageDirectives.ToList();

        packages.Should().HaveCount(2);
        packages[0].PackageInfo!.Name.Should().Be("My.Package");
        packages[1].PackageInfo!.Name.Should().Be("Another.Package");
    }

    [Fact]
    public void PackageDirectives_NoPackageDirectives_ReturnsEmpty()
    {
        var wrapper = CreateWrapper("""
            #:property PublishAot=false
            #:sdk Microsoft.NET.Sdk.Web
            Console.WriteLine("Hello");
            """);

        wrapper.PackageDirectives.Should().BeEmpty();
    }

    [Fact]
    public void Update_ChangePackageVersion_UpdatesSyntaxTree()
    {
        var wrapper = CreateWrapper("""
            #:package FileBasedApp.Toolkit@1.0.0
            #:package Microsoft.CodeAnalysis.CSharp@4.0.0
            Console.WriteLine("Hello");
            """);

        foreach (var item in wrapper.PackageDirectives.Where(x => x.PackageInfo?.Name == "FileBasedApp.Toolkit"))
        {
            item.PackageInfo!.Version.Value.Should().Be("1.0.0");

            item.PackageInfo.Version.Value = "0.17.1-alpha-02";

            var updated = item.Update(wrapper.CompilationUnitSyntax);
            wrapper.CompilationUnitSyntax = updated;
        }

        var result = wrapper.CompilationUnitSyntax.ToFullString();

        result.Should().Contain("FileBasedApp.Toolkit@0.17.1-alpha-02");
        result.Should().Contain("Microsoft.CodeAnalysis.CSharp@4.0.0");
    }

    [Fact]
    public void Update_VersionAlreadyCorrect_NoChange()
    {
        var wrapper = CreateWrapper("""
            #:package FileBasedApp.Toolkit@0.17.1-alpha-02
            Console.WriteLine("Hello");
            """);

        var directive = wrapper.PackageDirectives
            .Single(x => x.PackageInfo?.Name == "FileBasedApp.Toolkit");

        directive.PackageInfo!.Version.Value.Should().Be("0.17.1-alpha-02");

        var originalText = wrapper.CompilationUnitSyntax.ToFullString();

        // No update needed — version already matches
        var result = directive.Update(wrapper.CompilationUnitSyntax).ToFullString();

        result.Should().Be(originalText);
    }
}
