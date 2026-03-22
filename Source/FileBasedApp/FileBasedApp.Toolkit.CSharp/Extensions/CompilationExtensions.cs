using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;

public static class NamedTypeSymbolExtensions
{
    
}

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

        public IEnumerable<INamedTypeSymbol> GetNamedTypeSymbolForCurrentAssembly()
        {
            var getNamedTypeSymbolsVisitor = new GetNamedTypeSymbolsVisitor();
            return getNamedTypeSymbolsVisitor.VisitAsEnumerable(compilation.Assembly);
        }

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