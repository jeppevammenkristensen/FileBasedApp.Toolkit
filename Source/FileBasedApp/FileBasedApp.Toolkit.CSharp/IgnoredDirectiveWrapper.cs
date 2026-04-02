using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FileBasedApp.Toolkit.CSharp;

internal record IgnoredDirectiveWrapper(FileBasedDirectiveType Directive, string Content, SyntaxTrivia trivia)
{
    public static IEnumerable<IgnoredDirectiveWrapper> FromSyntaxTriva(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.IgnoredDirectiveTrivia))
        {
            if (trivia.GetStructure() is IgnoredDirectiveTriviaSyntax ignoredDirectiveTriviaSyntax)
            {
                if (CSharpRegex.HasFileBasedDirectiveRegex.Match(ignoredDirectiveTriviaSyntax.Content.Text) is
                    {Success: true} match)
                {
                    FileBasedDirectiveType directive = match.Value switch
                    {
                        "package" => FileBasedDirectiveType.Package,
                        "property" => FileBasedDirectiveType.Property,
                        "sdk" => FileBasedDirectiveType.Sdk,
                        "project" => FileBasedDirectiveType.Project,
                        _ => FileBasedDirectiveType.Unknown
                    };
                    yield return new IgnoredDirectiveWrapper(directive, ignoredDirectiveTriviaSyntax.Content.Text, trivia);
                }
            }
        }
    }
}