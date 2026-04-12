using Vogen;

namespace FileBasedApp.Toolkit;

/// <summary>
/// An UriFragment. Can be called with or without the leading #
/// </summary>
/// <remarks><see cref="Value"/> will return a value starting with /</remarks>
[ValueObject<string>]
public partial class UriFragment
{
    /// <summary>
    /// Gets a value indicating whether the fragment is empty or contains only whitespace characters.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
    
    private static string NormalizeInput(string input) =>
        string.IsNullOrWhiteSpace(input) ? string.Empty : "#" + input.TrimStart('#');

    /// <summary>
    /// returns the Fragment without '#'
    /// </summary>
    public string ValueWithHashtag => Value.TrimStart('#');
}