
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Wraps a package directive from a file-based app, providing access to parsed package information
/// and the ability to update the directive within a compilation unit.
/// </summary>
public class PackageDirectiveWrapper
{
    private readonly IgnoredDirectiveWrapper _wrapper;

    /// <summary>
    /// Gets or sets the parsed package name and version information from the directive content.
    /// Returns <see langword="null"/> if the content could not be parsed.
    /// </summary>
    public PackageInfo? PackageInfo { get; set; }

    internal PackageDirectiveWrapper(IgnoredDirectiveWrapper wrapper)
    {
        if (wrapper.Directive != FileBasedDirectiveType.Package)
        {
            throw new ArgumentException("The provided wrapper does not contain a package directive");
        }
        _wrapper = wrapper;
        PackageInfo = PackageInfo.SafeParse(_wrapper.Content);
    }

    /// <summary>
    /// Updates the compilation unit syntax with the values based on the package directive
    /// </summary>
    /// <param name="compilationUnitSyntax"></param>
    /// <returns></returns>
    public CompilationUnitSyntax Update(CompilationUnitSyntax compilationUnitSyntax)
    {
        var content = PackageInfo is not null
            ? $"package {PackageInfo.Name}@{PackageInfo.Version.Value}"
            : _wrapper.Content;
        var originalText = _wrapper.trivia.ToFullString();
        var lineEnding = originalText.EndsWith("\r\n") ? "\r\n" : "\n";
        var syntaxTriviaList = SyntaxFactory.ParseLeadingTrivia($"#:{content}{lineEnding}");
        return compilationUnitSyntax.ReplaceTrivia(_wrapper.trivia, syntaxTriviaList);
    }
}