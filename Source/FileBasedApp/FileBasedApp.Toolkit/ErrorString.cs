namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a typed instance of an error
/// </summary>
/// <param name="Error">The error</param>]
public record ErrorString(string Error)
{
    /// <summary>
    /// Implicit operator for converting a string to an ErrorString
    /// /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator ErrorString(string error) => new(error);
}