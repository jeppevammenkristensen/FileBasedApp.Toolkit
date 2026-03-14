namespace FileBasedApp.Toolkit;

internal static class StringExtensions
{
    /// <summary>
    /// Joins the elements of a string collection into a single string using the specified separator between each element.
    /// </summary>
    /// <param name="strings">The collection of strings to join.</param>
    /// <param name="separator">The string to use as a separator between elements.</param>
    /// <returns>A string that consists of the elements in the collection delimited by the separator string. If the collection is empty, the method returns an empty string.</returns>
    public static string StringJoin(this IEnumerable<string> strings, string separator) =>
        string.Join(separator, strings);
}