using System.Diagnostics.CodeAnalysis;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents an absolute web URI (HTTP/HTTPS). Wraps <see cref="Uri"/> with validation that ensures the URI is absolute.
/// </summary>
public class AbsoluteWebUri : AbstractUri<AbsoluteWebUri>, IParsable<AbsoluteWebUri>, IWebUri<AbsoluteWebUri>
{
    /// <inheritdoc />
    protected override Uri FullUriRepresentation => Uri;

    /// <summary>
    /// Initializes a new instance of <see cref="AbsoluteWebUri"/> with the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The absolute URI to wrap.</param>
    protected AbsoluteWebUri(Uri uri) : base(uri)
    {
    }
    

    /// <summary>
    /// Creates an <see cref="AbsoluteWebUri"/> from a URL string.
    /// </summary>
    /// <param name="url">The URL string to parse.</param>
    /// <returns>A new <see cref="AbsoluteWebUri"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="url"/> is not a valid URI.</exception>
    public static AbsoluteWebUri Create(string url)
    {
        var uri = url.SafeParseUri() ?? throw new InvalidOperationException($"The provided string '{url}' is not a valid URI");
        return Create(uri);
    }

    /// <summary>
    /// Creates an <see cref="AbsoluteWebUri"/> from a <see cref="Uri"/> instance.
    /// </summary>
    /// <param name="uri">The URI to wrap.</param>
    /// <returns>A new <see cref="AbsoluteWebUri"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="uri"/> is not an absolute URI.</exception>
    public static AbsoluteWebUri Create(Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            return new AbsoluteWebUri(uri);
        }

        throw new ArgumentException("The provided URI is not absolute", nameof(uri));
    }


    /// <inheritdoc />
    public override string Value => Uri.ToString();

    /// <inheritdoc />
    protected override AbsoluteWebUri CreateFromRelativeUrl(Uri uri)
    {
        return new AbsoluteWebUri(new Uri(Uri, uri));
    }

    /// <summary>
    /// Combines an <see cref="AbsoluteWebUri"/> with a relative path segment.
    /// </summary>
    /// <param name="left">The base URI.</param>
    /// <param name="right">The relative path segment to append.</param>
    /// <returns>A new <see cref="AbsoluteWebUri"/> with the combined path.</returns>
    public static AbsoluteWebUri operator /(AbsoluteWebUri left, string right)
    {
        var combinedUri = new Uri(left.Uri, right);
        return new AbsoluteWebUri(combinedUri);
    }

    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="relativePath"></param>
    // /// <returns></returns>
    // public AbsoluteWebUri Combine(string relativePath)
    // {
    //     
    // }

    /// <summary>
    /// Parses a string into an <see cref="AbsoluteWebUri"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (unused).</param>
    /// <returns>A new <see cref="AbsoluteWebUri"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid absolute URI.</exception>
    public static AbsoluteWebUri Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result)) return result;
        throw new FormatException();
    }

    /// <summary>
    /// Tries to parse the input string as an absolute web URI. Returns true if successful, false otherwise.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="provider"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AbsoluteWebUri result)
    {
        if (Uri.TryCreate(s, UriKind.Absolute, out var uri))
        {
            result = new AbsoluteWebUri(uri);
            return true;
        }

        result = null!;
        return false;
    }


    /// <summary>
    /// Implicitly converts an <see cref="AbsoluteWebUri"/> to a <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The <see cref="AbsoluteWebUri"/> to convert.</param>
    /// <returns>The underlying <see cref="Uri"/> instance.</returns>
    public static implicit operator Uri(AbsoluteWebUri uri)
    {
        return uri.Uri;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}