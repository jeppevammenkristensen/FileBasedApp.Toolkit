namespace FileBasedApp.Toolkit;

/// <summary>
/// Checks only if the string reference is null, without validating whether it is empty or contains only whitespace characters.
/// </summary>
public enum StringNullCheck
{
    /// <summary>
    /// Checks only if the string reference is null, without validating whether it is empty or contains
    /// </summary>
    Null,

    /// <summary>
    /// Checks if the string reference is null or an empty string.
    /// </summary>
    NullOrEmpty,

    /// <summary>
    /// Checks if the string reference is null or contains only whitespace characters.
    /// </summary>
    NullOrWhitespace
}