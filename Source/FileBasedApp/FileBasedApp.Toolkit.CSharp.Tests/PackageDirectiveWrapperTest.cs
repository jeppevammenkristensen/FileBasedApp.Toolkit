using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FileBasedApp.Toolkit.CSharp.Tests;

[TestSubject(typeof(PackageDirectiveWrapper))]
public class PackageDirectiveWrapperTest
{

    [Fact]
    public void Update_ReplacesPackageDirectiveTriviaWithNormalizedTrivia()
    {
        var original = SyntaxFactory.ParseCompilationUnit("""
#:package My.Package@1.2.3
Console.WriteLine("Hello");
""");

        var trivia = original.GetLeadingTrivia().First();
        var wrapper = CreatePackageDirectiveWrapper(trivia, "My.Package@2.0.0");
        var sut = (PackageDirectiveWrapper)wrapper;

        var updated = sut.Update(original);

        Assert.Equal("""
#:My.Package@2.0.0
Console.WriteLine("Hello");
""", updated.ToFullString());
        Assert.Equal($"#:My.Package@2.0.0{Environment.NewLine}", updated.GetLeadingTrivia().First().ToFullString());
    }

    [Fact]
    public void Update_WithMultipleDirectives_ReplacesOnlyTargetedDirective()
    {
        var original = SyntaxFactory.ParseCompilationUnit("""
#:package First.Package@1.0.0
#:package Second.Package@1.0.0
#:package Third.Package@1.0.0
Console.WriteLine("Hello");
""");

        var secondTrivia = original.GetLeadingTrivia()
            .Where(t => t.IsKind(SyntaxKind.IgnoredDirectiveTrivia))
            .ElementAt(1);

        var sut = CreatePackageDirectiveWrapper(secondTrivia, "Second.Package@2.0.0");

        var updated = sut.Update(original);

        Assert.Equal("""
#:package First.Package@1.0.0
#:Second.Package@2.0.0
#:package Third.Package@1.0.0
Console.WriteLine("Hello");
""", updated.ToFullString());
    }

    private static PackageDirectiveWrapper CreatePackageDirectiveWrapper(SyntaxTrivia trivia, string content)
    {
        return new PackageDirectiveWrapper(new IgnoredDirectiveWrapper(FileBasedDirectiveType.Package, content,
            trivia));
    }
}