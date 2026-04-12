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
    public Uri Uri { get;  }

    /// <summary>
    /// The full Uri representation. Relevant when the uri is relative as it otherwise will throw on most properties when
    /// they are requested
    /// </summary>
    protected abstract Uri FullUriRepresentation { get; }

    /// <summary>
    /// Provides a base implementation for URI types with common functionality for manipulating paths, query strings, and fragments.
    /// </summary>
    /// <typeparam name="TSelf">The concrete type that implements this abstract class, enabling fluent method chaining with the derived type.</typeparam>
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
    public TSelf AddPathSegment(string segment)
    {
        var oldPath = FullUriRepresentation.AbsolutePath;
        var newPath = oldPath.TrimEnd('/') + "/" + segment;
        RelativeWebUri.Create(newPath);

        return NewPath(newPath, FullUriRepresentation.Query, FullUriRepresentation.Fragment);
    }

    /// <summary>
    /// Creates and instance of <see cref="TSelf"/> from the relative url
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected abstract TSelf CreateFromRelativeUrl(Uri uri);
    

    /// <summary>
    /// Adds or replaces the current fragment and return a new <see cref="RelativeWebUri"/> with the fragment of the URI.
    /// </summary>
    /// <param name="fragment">The fragment. Can be called with or without the #</param>
    /// <returns></returns>
    public TSelf WithFragment(string fragment)
    {
        return NewPath(FullUriRepresentation.AbsolutePath, FullUriRepresentation.Query, "#" + fragment.TrimStart('#'));
    }
    
    /// <summary>
    /// Add or replace the current querystring and return a new <see cref="RelativeWebUri"/> with the querystring
    /// </summary>
    /// <param name="querystring">The querystring. Can be called with or with the leading ?</param>
    /// <returns></returns>
    public TSelf WithRawQuerystring(string querystring)
    {
        return NewPath(FullUriRepresentation.AbsolutePath, "?" + querystring.TrimStart('?'), FullUriRepresentation.Fragment);
    }
    
    /// <summary>
    /// Adds a query part to the current relative URI.
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="value">The value</param>
    /// <returns>A new instance of <see cref="RelativeWebUri"/> with the new query part added</returns>
    public TSelf AddQueryPart(string name, string value)
    {
        var result = HttpUtility.ParseQueryString(FullUriRepresentation.Query ?? string.Empty);
        
        result.Add(name, value);

        return NewPath(FullUriRepresentation.AbsolutePath, CreateQuery(result), FullUriRepresentation.Fragment);
    }

    private string CreateQuery(NameValueCollection query)
    {
        if (query.Count == 0) return string.Empty;

        var builder = new StringBuilder().Append("?");

        foreach (var key in query.AllKeys)
        {
            var value = query[key];
            builder.Append(key + "=" + value);
            builder.Append('&');
        }

        return builder.ToString().TrimEnd('&');
    }

    /// <summary>
    /// Constructs a new <see cref="TSelf"/> by combining the specified path, query string, and fragment components.
    /// </summary>
    /// <param name="path">The absolute path portion of the URI.</param>
    /// <param name="query">The query string portion of the URI, or null if no query string is present.</param>
    /// <param name="fragment">The fragment portion of the URI, or null if no fragment is present.</param>
    /// <returns>A new instance of <see cref="TSelf"/> with the combined path components.</returns>
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
}