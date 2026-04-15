namespace FileBasedApp.Toolkit;

/// <summary>
/// Adds extension methods to <see cref="AbsoluteWebUri"/> for HTTP requests.
/// </summary>
public static class AbsoluteWebUriHttpExtensions
{
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