namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extension methods for URI-related string conversions and operations.
/// </summary>
/// <remarks>
/// This static class contains utility methods for safely parsing URIs and converting strings
/// to specialized URI component types including query strings, fragments, and path segments.
/// Both nullable and required (non-null) variants are provided for flexibility in different scenarios.
/// </remarks>
internal static class UriExtensions
{
    /// <summary>
    /// Safely parses a URI string. Returns null if parsing fails.
    /// </summary>
    /// <param name="uriString"></param>
    /// <returns></returns>
    /// <remarks>Will swallow exceptions</remarks>
    internal static Uri? SafeParseUri(this string? uriString)
    {
        try
        {
            if (uriString is null) return null;
            return new Uri(uriString);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a nullable string to a UriQueryString. Returns null if the input string is null.
    /// </summary>
    /// <param name="value">The string value to convert to a UriQueryString.</param>
    /// <returns>A UriQueryString instance if the input is not null; otherwise, null.</returns>
    public static UriQueryString? ToQueryString(this string? value) => value == null ? null : UriQueryString.From(value); 
    
     /// <summary>
     /// Converts a string to a UriQueryString value object
     /// </summary>
     /// <param name="value">The string value to convert to a UriQueryString</param>
     /// <returns>A UriQueryString representation of the input string</returns>
     public static UriQueryString ToRequiredQueryString(this string? value) => value.ToQueryString() ?? throw new ArgumentNullException(nameof(value));

     /// <summary>
     /// Converts a string to a UriFragment. Returns null if the value is null.
     /// </summary>
     /// <param name="value">The string value to convert to a UriFragment.</param>
     /// <returns>A UriFragment instance if the value is not null; otherwise, null.</returns>
     public static UriFragment? ToFragment(this string? value) => value == null ? null : UriFragment.From(value);
     
     /// <summary>
     /// Converts a string to a UriFragment object.
     /// </summary>
     /// <param name="value">The string value to convert to a URI fragment.</param>
     /// <returns>A UriFragment instance created from the specified string value.</returns>
     public static UriFragment ToRequiredFragment(this string? value) => value.ToFragment() ?? throw new ArgumentNullException(nameof(value));

     /// <summary>
     /// Converts a string to a UriPathSegment.
     /// </summary>
     /// <param name="value">The string value to convert to a URI path segment.</param>
     /// <returns>A UriPathSegment instance created from the specified string value.</returns>
     public static UriPathSegment? ToPathSegment(this string? value) => value == null ? null : UriPathSegment.From(value);
     
     /// <summary>
     /// Converts a string to a UriPathSegment.
     /// </summary>
     /// <param name="value">The string value to convert to a URI path segment.</param>
     /// <returns>A UriPathSegment representing the converted string value.</returns>
     public static UriPathSegment ToRequiredPathSegment(this string value) => value.ToPathSegment() ?? throw new ArgumentNullException(nameof(value));
     
}