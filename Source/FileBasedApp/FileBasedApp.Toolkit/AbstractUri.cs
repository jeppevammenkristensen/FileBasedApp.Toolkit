using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace FileBasedApp.Toolkit;

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
    public Uri Uri { get; }

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
        var newPath = oldPath.TrimEnd('/') + segment;
        RelativeWebUri.Create(newPath);

        return NewPath(newPath, FullUriRepresentation.Query, FullUriRepresentation.Fragment);
    }

    /// <summary>
    /// Replaces all path segments in the current URI with the specified segment.
    /// for instance https://some.url/1/2 becomes https://some.url/3 if segment is 3
    /// </summary>
    /// <param name="segment"></param>
    /// <returns></returns>
    public TSelf WithPathSegment(UriPathSegment segment)
    {
        return NewPath(segment.Value, FullUriRepresentation.Query, FullUriRepresentation.Fragment);
    }

    /// <summary>
    /// Returns a new URI representing the parent of the current URI by dropping the last
    /// path segment while preserving the existing query string and fragment.
    /// </summary>
    /// <example>
    /// <c>https://some.url/parent/child?seg=1#frag</c> becomes <c>https://some.url/parent?seg=1#frag</c>.
    /// </example>
    /// <returns>A new <typeparamref name="TSelf"/> instance pointing at the parent path.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current URI has no path segments to remove (i.e. it is already at the root).
    /// </exception>
    public TSelf Parent()
    {
        // The first segment will always be / therefore we check for length of 1
        if (FullUriRepresentation.Segments.Length <= 1) 
        {
            throw new InvalidOperationException("Cannot get parent of root");
        }

        var newPathSegment = UriPathSegment.From(FullUriRepresentation.Segments[..^1].StringJoin("/").Trim('/'));
        return WithPathSegment(newPathSegment);
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

    /// <summary>
    /// Gets the fragment component of the URI, if present; otherwise, null.
    /// </summary>
    /// <remarks>
    /// The fragment is the portion of the URI that follows the '#' character.
    /// Returns null if no fragment component exists in the URI.
    /// </remarks>
    public UriFragment Fragment => FullUriRepresentation.Fragment.ToRequiredFragment();

    /// <summary>
    /// Gets the absolute path segment of the URI.
    /// </summary>
    public UriPathSegment PathSegment => FullUriRepresentation.AbsolutePath.ToRequiredPathSegment();

    /// <summary>
    /// Gets the query string component of the URI.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the URI does not contain a query string.</exception>
    public UriQueryString QueryString => FullUriRepresentation.Query.ToRequiredQueryString();





}