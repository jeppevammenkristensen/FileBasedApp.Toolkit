namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a web URI (HTTP/HTTPS).
/// </summary>
public interface IWebUri<TSelf> where TSelf : IWebUri<TSelf>
{
    /// <summary>
    /// Gets the underlying URI object representing the web address.
    /// </summary>
    public Uri Uri { get; }
    
    /// <summary>
    /// Gets the string representation of the URI.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an instance from the specified URI.
    /// </summary>
    /// <param name="uri">The URI to create the instance from.</param>
    /// <returns>A new instance of the implementing type.</returns>
    static abstract TSelf Create(string uri);
}