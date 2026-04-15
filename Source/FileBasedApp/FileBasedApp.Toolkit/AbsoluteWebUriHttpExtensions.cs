namespace FileBasedApp.Toolkit;

/// <summary>
/// Adds extension methods to <see cref="AbsoluteWebUri"/> for HTTP requests.
/// </summary>
public static class AbsoluteWebUriHttpExtensions
{
    /// <param name="uri">The absolute web URI to request.</param>
    extension<TSelf>(IWebUri<TSelf> uri) where TSelf : IWebUri<TSelf>
    {
        /// <summary>
        /// Sends a GET request to the specified absolute web URI and returns the response.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> used to send the request.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> returned by the server.</returns>
        public Task<HttpResponseMessage> GetAsync(HttpClient httpClient,
            CancellationToken cancellationToken = default)
        {
            return httpClient.GetAsync(uri.Uri, cancellationToken);
        }

        /// <summary>
        /// Applies this web URI as the base address of the specified <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure with this URI as its base address.</param>
        /// <returns>The current web URI instance to allow method chaining.</returns>
        public IWebUri<TSelf> ApplyAsBaseAddressTo(HttpClient client)
        {
            client.WithBaseAddress(uri);
            return uri;
        }
    }

    extension(HttpClient client)
    {
        /// <summary>
        /// Sets the base address of the <see cref="HttpClient"/> to the specified absolute web URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="TSelf"></typeparam>
        /// <returns></returns>
        public HttpClient WithBaseAddress<TSelf>(IWebUri<TSelf> uri) where TSelf : IWebUri<TSelf>
        {
            client.BaseAddress = uri.Uri;
            return client;
        }
    }
}