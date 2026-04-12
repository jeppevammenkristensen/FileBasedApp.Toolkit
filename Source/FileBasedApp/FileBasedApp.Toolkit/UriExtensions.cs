namespace FileBasedApp.Toolkit;

internal static class UriExtensions
{
    /// <summary>
    /// Safely parses a URI string. Returns null if parsing fails.
    /// </summary>
    /// <param name="uriString"></param>
    /// <returns></returns>
    /// <remarks>Will swallow exceptions</remarks>
    internal static Uri? SafeParseUri(this string? uriString)
    {
        try
        {
            if (uriString is null) return null;
            return new Uri(uriString);
        }
        catch (Exception)
        {
            return null;
        }
       
    }
}