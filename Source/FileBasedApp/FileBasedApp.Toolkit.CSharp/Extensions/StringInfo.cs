using Microsoft.CodeAnalysis;

namespace FileBasedApp.Toolkit.CSharp.Extensions;

/// <summary>
/// Describes whether a type symbol represents a string or <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.
/// </summary>
/// <param name="IsStringValue">Whether the type is <see cref="string"/>.</param>
/// <param name="IsReadOnlySpan">Whether the type is <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.</param>
public record StringInfo(bool IsStringValue, bool IsReadOnlySpan)
{
    /// <summary>Gets whether the type is string-like (either a string or a ReadOnlySpan of char).</summary>
    public bool IsStringLike => IsStringValue || IsReadOnlySpan;

    /// <summary>Converts to <see langword="true"/> when the type is string-like.</summary>
    public static implicit operator bool(StringInfo info) => info.IsStringLike;

    /// <summary>A <see cref="StringInfo"/> representing <see cref="string"/>.</summary>
    public static StringInfo CreateString { get; } = new(true, false);

    /// <summary>A <see cref="StringInfo"/> representing <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.</summary>
    public static StringInfo CreateReadOnlySpan { get; } = new(false, true);
    /// <summary>A <see cref="StringInfo"/> representing a non-string type.</summary>
    public static StringInfo CreateNotAString { get; } = new(false, false);

    /// <summary>Gets whether the type has a nullable annotation.</summary>
    public bool IsNull { get; private set; }

    /// <summary>Sets the <see cref="IsNull"/> flag and returns this instance for fluent chaining.</summary>
    public StringInfo SetNull(bool isNull)
    {
        IsNull = isNull;
        return this;
    }
}

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
