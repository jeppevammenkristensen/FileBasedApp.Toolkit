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