using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vogen;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a URI query string component. Can be called with or without a leading ? 
/// </summary>
/// <remarks>The <see cref="Value"/> return will always be with a leading ? </remarks>
[ValueObject<string>]
public partial struct UriQueryString
{
    private static string NormalizeInput(string input)
    {
        return "?" + input.TrimStart('?');
    }
}

/// <summary>
/// An UriFragment. Can be called with or without the leading #
/// </summary>
/// <remarks><see cref="Value"/> will return a value starting with /</remarks>
[ValueObject<string>]
public partial struct UriFragment
{
    private static string NormalizeInput(string input) =>
        "#" + input.TrimStart('#');
}

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
public readonly partial struct UriPathSegment
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


/// <summary>
/// Provides an abstract base class for URI manipulation with support for path segments, query strings, and fragments.
/// This class serves as a foundation for implementing specific URI types with immutable operations.
/// </summary>
/// <typeparam name="TSelf">The derived type that implements this abstract class, enabling fluent method chaining with the correct return type.</typeparam>
/// <remarks>
/// This abstract class implements common URI manipulation operations while delegating type-specific behavior to derived classes.
/// All mutation operations return new instances rather than modifying the existing instance, ensuring immutability.
/// The class uses the self-referential generic type pattern to enable derived classes to return their own type from fluent methods.
/// </remarks>
public abstract class AbstractUri<TSelf> where TSelf : IWebUri<TSelf>
{
    /// <summary>
    /// An Uri representation of the uri
    /// </summary>
    public Uri Uri { get;  }

    /// <summary>
    /// The full Uri representation. Relevant when the uri is relative as it otherwise will throw on most properties when
    /// they are requested
    /// </summary>
    protected abstract Uri FullUriRepresentation { get; }


    
    /// <summary>
    /// Abstract base class for URI manipulation providing common functionality for building and modifying URIs.
    /// Supports path segments, query strings, and fragments with a fluent interface pattern.
    /// </summary>
    /// <param name="uri">The Uri associated </param>
    protected AbstractUri(Uri uri)
    {
        Uri = uri;
    }
    
    /// <summary>
    /// Returns the string representation of the Uri.
    /// </summary>
    public virtual string Value => Uri.ToString();
    
    /// <summary>
    /// Gets a value indicating whether the relative URI contains a query string.
    /// </summary>
    public bool HasQuery => FullUriRepresentation.Query.Length > 0;

    /// <summary>
    /// Gets a value indicating whether the relative URI contains a fragment.
    /// </summary>
    public bool HasFragments => FullUriRepresentation.Fragment.Length > 0;
    
    /// <summary>
    /// Adds a path segment to the current relative URI path, preserving any existing query string and fragment.
    /// </summary>
    /// <param name="segment">The path segment to append to the current URI path.</param>
    /// <returns>A new <see cref="RelativeWebUri"/> instance with the added path segment.</returns>
    public TSelf AddPathSegment(UriPathSegment segment)
    {
        var oldPath = FullUriRepresentation.AbsolutePath;
        var newPath = oldPath.TrimEnd('/') + "/" + segment;
        RelativeWebUri.Create(newPath);

        return NewPath(newPath, FullUriRepresentation.Query, FullUriRepresentation.Fragment);
    }

    /// <summary>
    /// Creates and instance from the relative url
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected abstract TSelf CreateFromRelativeUrl(Uri uri);
    

    /// <summary>
    /// Adds or replaces the current fragment and return a new <see cref="RelativeWebUri"/> with the fragment of the URI.
    /// </summary>
    /// <param name="fragment">The fragment. Can be called with or without the #</param>
    /// <returns></returns>
    public TSelf WithFragment(UriFragment fragment)
    {
        return NewPath(FullUriRepresentation.AbsolutePath, FullUriRepresentation.Query, fragment.Value);
    }
    
    /// <summary>
    /// Add or replace the current querystring and return a new <see cref="RelativeWebUri"/> with the querystring
    /// </summary>
    /// <param name="querystring">The full querystring</param>
    /// <returns></returns>
    public TSelf WithRawQuerystring(UriQueryString querystring)
    {
        return NewPath(FullUriRepresentation.AbsolutePath, querystring.Value, FullUriRepresentation.Fragment);
    }
    
    /// <summary>
    /// Adds a query part to the current relative URI.
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="value">The value</param>
    /// <returns>A new instance of <see cref="RelativeWebUri"/> with the new query part added</returns>
    public TSelf AddQueryPart(string name, string value)
    {
        var result = HttpUtility.ParseQueryString(FullUriRepresentation.Query);
        
        result.Add(name, value);

        return NewPath(FullUriRepresentation.AbsolutePath, CreateQuery(result), FullUriRepresentation.Fragment);
    }

    private string CreateQuery(NameValueCollection query)
    {
        if (query.Count == 0) return string.Empty;

        var builder = new StringBuilder().Append('?');

        foreach (var key in query.AllKeys)
        {
            var value = query[key];
            builder.Append(key + "=" + value);
            builder.Append('&');
        }

        return builder.ToString().TrimEnd('&');
    }

    /// <summary>
    /// Constructs a new instance by combining the specified path, query string, and fragment components.
    /// </summary>
    /// <param name="path">The absolute path portion of the URI.</param>
    /// <param name="query">The query string portion of the URI, or null if no query string is present.</param>
    /// <param name="fragment">The fragment portion of the URI, or null if no fragment is present.</param>
    /// <returns>A new instance with the combined path components.</returns>
    protected TSelf NewPath(string path, string? query, string? fragment)
    {
        var builder = new StringBuilder(path);
        if (query != null)
        {
            builder.Append(query);
        }
        if (fragment != null)
        {
            builder.Append(fragment);
        }
        return CreateFromRelativeUrl(RelativeWebUri.Create(builder.ToString()).Uri);
    }
    
    /// <summary>
    /// Divides the current URI by a path segment, creating a new URI with the segment appended to the path.
    /// </summary>
    /// <param name="uri">The current URI instance.</param>
    /// <param name="segment">The path segment to append.</param>
    /// <returns>A new instance with the appended path segment.</returns>
    public static TSelf operator /(AbstractUri<TSelf> uri, UriPathSegment segment)
    {
        return uri.AddPathSegment(segment);
    }
    
    /// <summary>
    /// Divides the current URI by a path segment, creating a new URI with the segment appended to the path.
    /// </summary>
    /// <param name="uri">The current URI instance.</param>
    /// <param name="segment">The path segment to append.</param>
    /// <returns>A new instance with the appended path segment.</returns>
    public static TSelf operator /(AbstractUri<TSelf> uri, UriFragment segment)
    {
        return uri.WithFragment(segment);
    }

    /// <summary>
    /// Adds a query string to the URI using the division operator
    /// </summary>
    /// <param name="uri">The base URI to add the query string to</param>
    /// <param name="segment">The query string to append to the URI</param>
    /// <returns>A new URI instance with the query string applied</returns>
    public static TSelf operator /(AbstractUri<TSelf> uri, UriQueryString segment) => uri.WithRawQuerystring(segment);
    
    
}