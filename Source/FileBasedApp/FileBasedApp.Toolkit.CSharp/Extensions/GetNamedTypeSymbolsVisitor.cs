using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;

internal class GetNamedTypeSymbolsVisitor : BaseSymbolVisitor<INamedTypeSymbol>
{
    protected override void HandleNamedTypeSymbol(INamedTypeSymbol symbol)
    {
        ExecuteMatched(symbol);
    }
}