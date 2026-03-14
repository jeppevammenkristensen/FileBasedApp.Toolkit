using Microsoft.CodeAnalysis;

namespace GetFileSystemSourceAndParse.Extensions;

public record StringInfo(bool IsStringValue, bool IsReadOnlySpan)
{
    public bool IsStringLike => IsStringValue || IsReadOnlySpan;
    
    public static implicit operator bool(StringInfo info) => info.IsStringLike;

    public static StringInfo CreateString { get; } = new(true, false);

    public static StringInfo CreateReadOnlySpan { get; } = new(false, true);
    public static StringInfo CreateNotAString { get; } = new(false, false);
    
    public bool IsNull { get; private set; }

    public StringInfo SetNull(bool isNull)
    {
        IsNull = isNull;
        return this;
    }
}

public record EnumerableInfo(bool IsEnumerable, ITypeSymbol ElementType, bool IsAsyncEnumerable)
{
    public static implicit operator bool(EnumerableInfo info) => info.IsEnumerable;
    
    public static EnumerableInfo False { get; } = new(false, null!, false);
}