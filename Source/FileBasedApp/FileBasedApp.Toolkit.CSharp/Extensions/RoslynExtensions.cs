using Microsoft.CodeAnalysis;
using Roslynator;

namespace FileBasedApp.Toolkit.CSharp.Extensions;

/// <summary>
/// Extension methods for Roslyn <see cref="ITypeSymbol"/> to classify types as string-like, task-like, or enumerable.
/// </summary>
public static class RoslynExtensions
{

    /// <summary>
    /// Determines whether the type symbol represents a string-like type (<see cref="string"/> or <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>).
    /// </summary>
    public static StringInfo IsStringLike(this ITypeSymbol? typeSymbol, Compilation compilation)
    {
        var isNullable = typeSymbol?.NullableAnnotation == NullableAnnotation.Annotated;


        if (typeSymbol?.IsString() == true)
        {
            return StringInfo.CreateString.SetNull(isNullable);
        }

        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedType)
        {
            return StringInfo.CreateNotAString;
        }

        var readOnlySpanType = compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
        if (readOnlySpanType is null)
            return StringInfo.CreateNotAString;

        if (!namedType.ConstructedFrom.Equals(readOnlySpanType, SymbolEqualityComparer.Default))
        {
            return StringInfo.CreateNotAString;
        }

        return namedType.TypeArguments[0].SpecialType == SpecialType.System_Char
            ? StringInfo.CreateReadOnlySpan.SetNull(isNullable)
            : StringInfo.CreateNotAString;
    }


    /// <summary>
    /// Determines whether the type symbol represents a Task-like type (Task, Task&lt;T&gt;, ValueTask, or ValueTask&lt;T&gt;).
    /// </summary>
    public static bool IsTaskLike(this ITypeSymbol? typeSymbol, Compilation compilation)
    {
        if (typeSymbol is null)
            return false;

        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var valueTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        // Direct match (Task / ValueTask)
        if (SymbolEqualityComparer.Default.Equals(typeSymbol, taskType) ||
            SymbolEqualityComparer.Default.Equals(typeSymbol, valueTaskType))
        {
            return true;
        }

        // Generic match (Task<T> / ValueTask<T>)
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType)
        {
            var originalDefinition = namedType.OriginalDefinition;

            return SymbolEqualityComparer.Default.Equals(originalDefinition, taskOfT) ||
                   SymbolEqualityComparer.Default.Equals(originalDefinition, valueTaskOfT);
        }

        return false;
    }

    /// <summary>
    /// Attempts to extract the element type from an enumerable or async enumerable type symbol.
    /// </summary>
    public static EnumerableInfo TryGetEnumerableElementType(
        this ITypeSymbol? type,
        Compilation compilation)

    {
        ITypeSymbol elementType = null!;

        if (type == null)
            return EnumerableInfo.False;

        if (type is IArrayTypeSymbol arrayType)
        {
            elementType = arrayType.ElementType;
            return new EnumerableInfo(true, elementType, false);
        }

        var ienumerableOfT =
            compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");

        var iasyncenumerableOfT = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");

        if (ienumerableOfT == null)
            return EnumerableInfo.False;

        EnumerableInfo TryGetEnumerableOrAsyncEnumerable(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType)
            {
                elementType = namedType.TypeArguments[0];

                if (SymbolEqualityComparer.Default.Equals(
                        namedType.OriginalDefinition,
                        ienumerableOfT))
                {
                    return new EnumerableInfo(true, elementType, false);
                }

                if (namedType.OriginalDefinition.Equals(iasyncenumerableOfT, SymbolEqualityComparer.Default))
                {
                    return new EnumerableInfo(true, elementType, true);
                }
            }

            return EnumerableInfo.False;
        }

        if (TryGetEnumerableOrAsyncEnumerable(type) is {IsEnumerable: true} result)
        {
            return result;
        }


        foreach (var iface in type.AllInterfaces)
        {
            if (TryGetEnumerableOrAsyncEnumerable(iface) is {IsEnumerable: true} ifaceResult)
            {
                return ifaceResult;
            }
        }

        return EnumerableInfo.False;
    }
}
