using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// A base <see cref="SymbolVisitor"/> that recursively traverses a Roslyn symbol tree
/// and emits results of type <typeparamref name="TResultType"/> via a callback action.
/// </summary>
/// <typeparam name="TResultType">The type of result emitted during symbol visitation.</typeparam>
public abstract class BaseSymbolVisitor<TResultType> : SymbolVisitor
{
    private Action<TResultType>? _callback;
    
    /// <inheritdoc />
    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        HandleNamespace(symbol);
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    /// <inheritdoc />
    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        HandleNamedTypeSymbol(symbol);

        foreach (var namedTypeSymbol in symbol.GetTypeMembers())
        {
            namedTypeSymbol.Accept(this);
        }

        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    /// <inheritdoc />
    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        HandleAssembly(symbol);
        symbol.GlobalNamespace.Accept(this);
    }

    /// <summary>
    /// Called when a named type symbol is visited. Override to inspect or emit results for named types.
    /// </summary>
    /// <param name="symbol">The named type symbol being visited.</param>
    protected virtual void HandleNamedTypeSymbol(INamedTypeSymbol symbol)
    {
    }

    /// <inheritdoc />
    public override void VisitModule(IModuleSymbol symbol)
    {
        HandleModule(symbol);
        symbol.GlobalNamespace.Accept(this);
    }

    /// <summary>
    /// Called when an assembly symbol is visited. Override to inspect or emit results for assemblies.
    /// </summary>
    /// <param name="symbol">The assembly symbol being visited.</param>
    protected virtual void HandleAssembly(IAssemblySymbol symbol)
    {

    }

    /// <summary>
    /// Called when a module symbol is visited. Override to inspect or emit results for modules.
    /// </summary>
    /// <param name="symbol">The module symbol being visited.</param>
    protected virtual void HandleModule(IModuleSymbol symbol)
    {

    }

    /// <summary>
    /// Called when a namespace symbol is visited. Override to inspect or emit results for namespaces.
    /// </summary>
    /// <param name="symbol">The namespace symbol being visited.</param>
    protected virtual void HandleNamespace(INamespaceSymbol symbol)
    {

    }
    
    /// <summary>
    /// Calls this to signal a "match" was found. In combination with extension methods this can be used to create
    /// an enumerable result
    /// </summary>
    /// <param name="result">The data to emit</param>
    /// <remarks>This is the mechanism to report that a new item was found used by the <see cref="VisitAsEnumerable"/></remarks>
    protected void ExecuteMatched(TResultType result) => _callback?.Invoke(result);

    /// <summary>
    /// Visits the specified symbol and its descendants asynchronously, returning matched results as an enumerable sequence.
    /// The traversal is performed on a background task, and results are yielded as they are discovered.
    /// </summary>
    /// <param name="symbol">The root symbol to begin visitation from.</param>
    /// <return>An enumerable sequence of results produced during the symbol tree traversal.</return>
    /// <remarks>This uses a little trickery to produce an IEnumerable.</remarks>
    public IEnumerable<TResultType> VisitAsEnumerable(ISymbol symbol)
    {
        var collection = new BlockingCollection<TResultType>();
        _callback = item => collection.Add(item);
        
        _ = Task.Run(() =>
        {
            try
            {
                Visit(symbol);
            }
            finally
            {
                collection.CompleteAdding();
            }
        });
        
        return collection.GetConsumingEnumerable();
    }
}