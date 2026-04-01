using FileBasedApp.Toolkit.CSharp.Extensions;
using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp;


/// <summary>
/// Provides extension methods for <see cref="CSharpProjectAnalysis"/> to enhance its functionality.
/// </summary>
/// <remarks>
/// This static class contains utility extension methods that extend the capabilities of the CsharpProjectLoader class.
/// </remarks>
public static class CSharpProjectLoaderExtensions
{
    /// <summary>
    /// Extension methods for <see cref="CSharpProjectAnalysis"/>
    /// </summary>
    /// <param name="cls"></param>
    extension(CSharpProjectAnalysis cls)
    {
        /// <summary>
        /// Finds all named type symbols that implement the specified interface by its fully qualified metadata name.
        /// The interface type is first resolved from the compilation using the provided name, then all implementations are searched.
        /// </summary>
        /// <param name="fullyQualifiedName">The fully qualified metadata name of the interface to search for implementations.</param>
        /// <param name="assemblyOnly">If true, searches only within the current assembly; if false, searches across all referenced assemblies as well.</param>
        /// <returns>An enumerable collection of named type symbols that implement the specified interface.</returns>
        /// <remarks>A one to one of this method is also available on a Compilation instance</remarks>
        public IEnumerable<INamedTypeSymbol> FindImplementationOfInterface(string fullyQualifiedName, bool assemblyOnly)
        {
            return cls.Compilation.FindImplementationOfInterface(fullyQualifiedName, assemblyOnly);
            
        }

        /// <summary>
        /// Finds all named type symbols that implement the specified interface.
        /// </summary>
        /// <param name="interface">The interface type symbol to search for implementations of.</param>
        /// <param name="assemblyOnly">If true, searches only within the current assembly; if false, searches within the current assembly and all referenced assemblies.</param>
        /// <returns>An enumerable collection of <see cref="INamedTypeSymbol"/> instances representing types that implement the specified interface.</returns>
        /// /// <remarks>A one to one of this method is also available on a Compilation instance</remarks>
        public IEnumerable<INamedTypeSymbol> FindImplementationOfInterface(ITypeSymbol @interface, bool assemblyOnly)
        {
            return cls.Compilation.FindImplementationOfInterface(@interface, assemblyOnly);
        } 
    }
}