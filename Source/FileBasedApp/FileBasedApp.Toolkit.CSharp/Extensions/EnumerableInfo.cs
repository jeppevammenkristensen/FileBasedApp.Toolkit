using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;

/// <summary>
/// Describes whether a type symbol represents an enumerable and, if so, its element type.
/// </summary>
/// <param name="IsEnumerable">Whether the type implements <see cref="System.Collections.Generic.IEnumerable{T}"/> or <see cref="System.Collections.Generic.IAsyncEnumerable{T}"/>.</param>
/// <param name="ElementType">The element type of the enumerable.</param>
/// <param name="IsAsyncEnumerable">Whether the enumerable is an async enumerable.</param>
public record EnumerableInfo(bool IsEnumerable, ITypeSymbol ElementType, bool IsAsyncEnumerable)
{
    /// <summary>Converts to <see langword="true"/> when the type is enumerable.</summary>
    public static implicit operator bool(EnumerableInfo info) => info.IsEnumerable;

    /// <summary>A <see cref="EnumerableInfo"/> representing a non-enumerable type.</summary>
    public static EnumerableInfo False { get; } = new(false, null!, false);
}