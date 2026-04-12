using Vogen;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a URI query string component. Can be called with or without a leading ? 
/// </summary>
/// <remarks>The <see cref="Value"/> return will always be with a leading ? </remarks>
[ValueObject<string>]
public partial class UriQueryString
{
    private static string NormalizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        
        return "?" + input.TrimStart('?');
    }
}