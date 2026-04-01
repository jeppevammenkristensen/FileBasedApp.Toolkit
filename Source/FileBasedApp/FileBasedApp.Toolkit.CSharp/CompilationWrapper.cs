using FileBasedApp.Toolkit.CSharp.Extensions;
using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Wraps a Roslyn <see cref="Value"/> instance to provide additional context or functionality.
/// </summary>
/// <remarks>
/// This wrapper class encapsulates a compilation object and can be extended to add
/// additional compilation-related functionality or metadata without modifying the core compilation.
/// </remarks>
public class CompilationWrapper
{
    private readonly IDictionary<string, ITypeSymbol> _foundTypes = new Dictionary<string, ITypeSymbol>();
    
    /// <summary>
    /// Gets the wrapped Roslyn compilation instance.
    /// </summary>
    /// <value>
    /// A <see cref="Value"/> object representing the compiled state of a C# project,
    /// including syntax trees, references, and semantic information.
    /// </value>
    public Compilation Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationWrapper"/> class.
    /// </summary>
    /// <param name="value">The Roslyn compilation instance to wrap.</param>
    public CompilationWrapper(Compilation value)
    {
        Value = value;
    }

    /// <summary>
    /// Tries to get a type symbol by its fully qualified name. If the type has previously been loaded it will
    /// be returned from the cache. Otherwise, it will be loaded from the compilation and cached.
    /// </summary>
    /// <param name="fullyQualifiedName"></param>
    /// <param name="failOnMultipleMatches"></param>
    /// <returns></returns>
    public ITypeSymbol GetOrFindRequiredTypeSymbol(string fullyQualifiedName, bool failOnMultipleMatches = true)
    {
        if (_foundTypes.TryGetValue(fullyQualifiedName, out var typeSymbol))
        {
            return typeSymbol;
        }

        var findRequiredTypeByMetadataName = Value.FindRequiredTypeByMetadataName(fullyQualifiedName, failOnMultipleMatches);
        _foundTypes.Add(fullyQualifiedName, findRequiredTypeByMetadataName);
        return findRequiredTypeByMetadataName;
    }
    
    /// <summary>
    /// Implicit conversion operator to allow implicit conversion from <see cref="Value"/> to <see cref="CompilationWrapper"/>.
    /// </summary>
    /// <param name="compilationWrapper"></param>
    /// <returns></returns>
    public static implicit operator Compilation(CompilationWrapper compilationWrapper) => compilationWrapper.Value; 
}