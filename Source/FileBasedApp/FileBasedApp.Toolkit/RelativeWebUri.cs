using System.Diagnostics.CodeAnalysis;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a relative web URI. Wraps <see cref="Uri"/> with validation that ensures the URI is relative.
/// </summary>
/// <remarks>Updating Path, Query or Fragment will always result in a new instance</remarks>
public class RelativeWebUri : AbstractUri<RelativeWebUri>, IParsable<RelativeWebUri>, IWebUri<RelativeWebUri>
{

    /// <inheritdoc />
    protected override Uri FullUriRepresentation => DummyAbsolutePath;

    private Uri DummyAbsolutePath { get; set; }

    

    /// <summary>
    /// Initializes a new instance of <see cref="RelativeWebUri"/> with the specified relative <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The relative URI to wrap.</param>
    internal RelativeWebUri(Uri uri) : base(uri)
    {
        // A Uri created for a relative url has some limits to what can be displayed without throwing an exception
        DummyAbsolutePath = new Uri(new Uri("https://dummyurl.dk"), uri);
    }

    /// <summary>
    /// Gets the string representation of the URI.
    /// </summary>
    public override string Value
    {
        get
        {
            var pathAndQuery = DummyAbsolutePath.PathAndQuery;
            return pathAndQuery + DummyAbsolutePath.Fragment;
        }
    }

    /// <summary>
    /// Creates a new instance from the specified relative URI.
    /// </summary>
    /// <param name="uri">The relative URI to create the instance from.</param>
    /// <returns>A new instance of the implementing type.</returns>
    protected override RelativeWebUri CreateFromRelativeUrl(Uri uri)
    {
        return new RelativeWebUri(uri);
    }

    /// <summary>
    /// Creates a new <see cref="RelativeWebUri"/> instance from the specified relative URL string.
    /// </summary>
    /// <param name="relativeUrl">The relative URL string to create a <see cref="RelativeWebUri"/> from.</param>
    /// <returns>A new <see cref="RelativeWebUri"/> instance representing the specified relative URL.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided string is not a valid relative URI.</exception>
    public static RelativeWebUri Create(string relativeUrl)
    {
        if (Uri.TryCreate(relativeUrl, UriKind.Relative, out var uri))
            return new RelativeWebUri(uri);
        
        throw new ArgumentException("The provided string is not a valid relative URI", nameof(relativeUrl));
    }

    /// <summary>
    /// Parses a string into a <see cref="RelativeWebUri"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information.</param>
    /// <returns>A new <see cref="RelativeWebUri"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid relative URI.</exception>
    public static RelativeWebUri Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result : throw new FormatException();
    }

    /// <summary>
    /// Attempts to parse a string into a RelativeWebUri instance.
    /// </summary>
    /// <param name="s">The string to parse as a relative URI.</param>
    /// <param name="provider">An object that provides culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the parsed RelativeWebUri if the parsing succeeded, or null if the parsing failed.</param>
    /// <returns>true if the string was successfully parsed as a relative URI; otherwise, false.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RelativeWebUri result)
    {
        if (Uri.TryCreate(s, UriKind.Relative, out var uri))
        {
            result = new RelativeWebUri(uri);
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }
}