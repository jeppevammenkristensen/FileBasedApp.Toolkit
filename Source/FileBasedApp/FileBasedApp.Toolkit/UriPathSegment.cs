using System.Text.RegularExpressions;
using Vogen;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a single segment of a URI path
/// </summary>
/// <example>
/// <![CDATA[UriPathSegmentFrom("first/second")]]>
/// <![CDATA[UriPathSegmentFrom("/first")]]>
/// </example>
/// <remarks>
/// A path segment is a portion of a URI path between separators (forward slashes).
/// This type provides methods to format the segment with leading and/or trailing separators as needed.
/// The default segment value returned is without separators by default, and methods are present for returning with leading and or trailing 
/// </remarks>
[ValueObject<string>]
public partial class UriPathSegment
{
    [GeneratedRegex(@"^/?[A-Za-z0-9\-._~!$&'()*+,;=:@%]+/?$")]
    private static partial Regex PathRegex();
    
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Path segment must not be empty or whitespace.");

        if (!PathRegex().IsMatch(value))
        {
            return Validation.Invalid($"Path segment {value} is not a valid path segment.");
        }
        
        return Validation.Ok;
    }

    private static string NormalizeInput(string input)
    {
        return input.TrimEnd('/');   
    }

    /// <summary>
    /// Returns the segment with a leading separator
    /// </summary>
    /// <returns></returns>
    public string WithLeadingSeparator() => "/" + this.Value;

    /// <summary>
    /// Returns the segment with a trailing seperator
    /// </summary>
    /// <returns></returns>
    public string WithTrailingSeparator() => Value + "/";

    /// <summary>
    /// Returns the segment with both a leading and trailing separator
    /// </summary>
    /// <return>A string representation of the segment with separators on both ends</return>
    public string WithLeadingAndTrailingSeparator() => "/" + Value + "/";
    
}