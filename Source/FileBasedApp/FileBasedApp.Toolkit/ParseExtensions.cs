using System.Diagnostics.CodeAnalysis;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extension methods for parsing string values into various types that implement the IParsable interface.
/// </summary>
public static class ParseExtensions
{
    /// <summary>
    /// Attempts to parse the specified string value into the target type using the specified format provider.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed value if the conversion succeeded, or null if the conversion failed.</param>
    /// <typeparam name="T">The target type that implements IParsable. Must be parsable from a string representation.</typeparam>
    /// <return>True if the value was successfully parsed; otherwise, false.</return>
    public static bool TryParseTo<T>(this string? value,
        [MaybeNullWhen(returnValue:false)] out T result) where T : IParsable<T>
    {
        return TryParseTo(value, null, out result);
    }

    /// <summary>
    /// Safely parses the specified string value into the target struct type, returning null if parsing fails.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="formatProvider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? SafeParseToStruct<T>(string? value, IFormatProvider? formatProvider = null)
        where T : struct, IParsable<T>
    {
        return TryParseTo(value, formatProvider, out T result) ? result : null;
    }

    /// <summary>
    /// Safely parses the specified string value into the target reference type, returning null if parsing fails.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <typeparam name="T">The target reference type that implements IParsable. Must be a class that is parsable from a string representation.</typeparam>
    /// <return>The parsed value if the conversion succeeded; otherwise, null.</return>
    public static T? SafeParseToClass<T>(string? value, IFormatProvider? formatProvider = null)
        where T : class, IParsable<T>
    {
        return TryParseTo(value, formatProvider, out T result) ? result : null;
    }

    /// <summary>
    /// Attempts to parse the specified string value into the target type using the specified format provider.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the parsed value if the conversion succeeded, or null if the conversion failed.</param>
    /// <typeparam name="T">The target type that implements IParsable. Must be parsable from a string representation.</typeparam>
    /// <return>True if the value was successfully parsed; otherwise, false.</return>
    public static bool TryParseTo<T>(this string? value, IFormatProvider? formatProvider,
        [MaybeNullWhen(returnValue:false)] out T result) where T : IParsable<T>
    {
        return T.TryParse(value, formatProvider, out result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="formatProvider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RequiredParse<T>(this string? value, IFormatProvider? formatProvider = null) where T : IParsable<T> 
    {
        if (T.TryParse(value, formatProvider, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Unable to parse the value '{value}' to type {typeof(T).FullName}.");
    }
        
}