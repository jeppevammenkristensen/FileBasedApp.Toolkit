namespace FileBasedApp.Toolkit;

public static class StringExtensions
{
    /// <summary>
    /// Joins the elements of a string collection into a single string using the specified separator between each element.
    /// </summary>
    /// <param name="strings">The collection of strings to join.</param>
    /// <param name="separator">The string to use as a separator between elements.</param>
    /// <returns>A string that consists of the elements in the collection delimited by the separator string. If the collection is empty, the method returns an empty string.</returns>
    public static string StringJoin(this IEnumerable<string> strings, string separator) =>
        string.Join(separator, strings);

    /// <summary>
    /// Determines whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>True if the value parameter is null, empty, or contains only white-space characters; otherwise, false.</returns>
    /// <remarks>This is a convenience method to call IsNullOrWhitespace</remarks>
    public static bool IsNullOrWhitespace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Determines whether the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>True if the value parameter is null or an empty string; otherwise, false.</returns>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}