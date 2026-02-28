using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GetFileSystemSourceAndParse;

public class TriviaVisitor : CSharpSyntaxRewriter
{
    public TriviaVisitor() : base(true)
    {
        
    }
    
    public bool HasIfDirectiveTrivia { get; private set; }

    public ElseDirectiveTriviaSyntax? ElseDirectiveTriviaSyntax { get; private set; }

    public override SyntaxNode? VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
    {
        HasIfDirectiveTrivia = true;
        return base.VisitIfDirectiveTrivia(node);
    }

    public override SyntaxNode? VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
    {
        return null;
    }

    
}