using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;



internal class GetNamedTypeSymbolsVisitor : BaseSymbolVisitor<INamedTypeSymbol>
{
    private readonly Action<INamedTypeSymbol> _action;

    
    protected override void HandleNamedTypeSymbol(INamedTypeSymbol symbol)
    {
        ExecuteMatched(symbol);
    }
}


public static class CompilationExtensions
{
    extension(Compilation compilation)
    {
        public IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols()
        {
            var getNamedTypeSymbolsVisitor = new GetNamedTypeSymbolsVisitor();
            return getNamedTypeSymbolsVisitor.VisitAsEnumerable(compilation.GlobalNamespace);
        }
    }
}