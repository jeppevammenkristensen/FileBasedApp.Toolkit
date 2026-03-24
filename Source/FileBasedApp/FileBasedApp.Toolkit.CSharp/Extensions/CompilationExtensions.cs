using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;
/// <summary>
/// Extension methods for compilation
/// </summary>
public static class CompilationExtensions
{
    extension(Compilation compilation)
    {
        /// <summary>
        /// Retrieves all named type symbols from the compilation, including those from referenced assemblies.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="INamedTypeSymbol"/> instances representing all named types
        /// (classes, interfaces, structs, enums, delegates, and records) found in the compilation's global namespace
        /// and its referenced assemblies.
        /// </returns>
        public IEnumerable<INamedTypeSymbol> GetNamedTypeSymbolsIncludingReferences()
        {
            var getNamedTypeSymbolsVisitor = new GetNamedTypeSymbolsVisitor();
            return getNamedTypeSymbolsVisitor.VisitAsEnumerable(compilation.GlobalNamespace);
        }

        /// <summary>
        /// Retrieves all named type symbols defined in the current compilation assembly only, excluding types from referenced assemblies.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="INamedTypeSymbol"/> instances representing all named types
        /// (classes, interfaces, structs, enums, delegates, and records) defined in the current assembly.
        /// </returns>
        public IEnumerable<INamedTypeSymbol> GetNamedTypeSymbolForCurrentAssembly()
        {
            var getNamedTypeSymbolsVisitor = new GetNamedTypeSymbolsVisitor();
            return getNamedTypeSymbolsVisitor.VisitAsEnumerable(compilation.Assembly);
        }

        /// <summary>
        /// Finds and returns a type symbol by its fully qualified metadata name, throwing an exception if the type is not found or if multiple matches are found when not allowed.
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified metadata name of the type to find, including namespace.</param>
        /// <param name="failOnMultipleMatches">Determines whether to throw an exception when multiple types match the given name. If false, returns the first match when multiple types are found.</param>
        /// <return>
        /// An <see cref="ITypeSymbol"/> representing the found type. Returns the first match if multiple types are found and <paramref name="failOnMultipleMatches"/> is false.
        /// </return>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no types are found matching the specified name, or when multiple types are found and <paramref name="failOnMultipleMatches"/> is true.
        /// </exception>
        public ITypeSymbol FindRequiredTypeByMetadataName(string fullyQualifiedName, bool failOnMultipleMatches)
        {
            var results = compilation.GetTypesByMetadataName(fullyQualifiedName);

            return results switch
            {
                {Length: 0} => throw new InvalidOperationException($"No types found for {fullyQualifiedName}"),
                {Length: 1} => results[0],
                _ when failOnMultipleMatches => throw new InvalidOperationException(
                    $"Multiple types found for {fullyQualifiedName}"),
                _ => results[0]
            };
        }

        /// <summary>
        /// Finds all named type symbols that implement the specified interface by its fully qualified metadata name.
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified metadata name of the interface type to search for implementations of.</param>
        /// <param name="assemblyOnly">If true, searches only within the current assembly; if false, searches within the current assembly and all referenced assemblies.</param>
        /// <returns>An enumerable collection of <see cref="INamedTypeSymbol"/> instances representing types that implement the specified interface.</returns>
        public IEnumerable<INamedTypeSymbol> FindImplementationOfInterface(string fullyQualifiedName, bool assemblyOnly)
        {
            var type = compilation.FindRequiredTypeByMetadataName(fullyQualifiedName, failOnMultipleMatches: true);
            return compilation.FindImplementationOfInterface(type, assemblyOnly);
        }
        
        /// <summary>
        /// Finds all named type symbols that implement the specified interface.
        /// </summary>
        /// <param name="interface">The interface type symbol to search for implementations of.</param>
        /// <param name="assemblyOnly">If true, searches only within the current assembly; if false, searches within the current assembly and all referenced assemblies.</param>
        /// <returns>An enumerable collection of <see cref="INamedTypeSymbol"/> instances representing types that implement the specified interface.</returns>
        public IEnumerable<INamedTypeSymbol> FindImplementationOfInterface(ITypeSymbol @interface, bool assemblyOnly)
        {
            if (@interface.TypeKind != TypeKind.Interface)
            {
                throw new InvalidOperationException($"The specified symbol {@interface.Name} is not an interface. But was {@interface.TypeKind}");
            }
            
            var namedTypeSymbols = assemblyOnly
                ? compilation.GetNamedTypeSymbolForCurrentAssembly()
                : compilation.GetNamedTypeSymbolsIncludingReferences();
            
            return namedTypeSymbols
                .Where(x => x.AllInterfaces.Any(y => SymbolEqualityComparer.Default.Equals(y, @interface)));
        }
    }
}