using System.Runtime.InteropServices;
using TruePath;

namespace FileBasedApp.Toolkit.CommandCli;

// [JsonSerializable(typeof(FlexibleFilePath))]
// [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
// public partial class AppContext : JsonSerializerContext
// {
// }

/// <summary>
/// Stores Windows and Unix path values for platform-specific configuration, including JSON files.
/// </summary>
/// <param name="Windows">The windows path</param>
/// <param name="Unix">The unix path</param>
public record FlexibleFilePath(string Windows, string Unix)
{
    /// <summary>
    /// Selects the current platform's path, preserving whether it is relative or absolute.
    /// </summary>
    /// <returns>The normalized Windows path on Windows, or the normalized Unix path elsewhere.</returns>
    /// <remarks>
    /// Relative paths are not resolved against the current directory. Windows rooted-without-drive
    /// and drive-relative paths are also preserved. Empty input represents the current directory.
    /// This method does not access the file system or require the path to exist.
    /// </remarks>
    public LocalPath GetAsLocalPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new LocalPath(Windows);
        }

        return new LocalPath(Unix);
    }

    /// <summary>
    /// Implicitly converts a FlexibleFilePath to a LocalPath by selecting the appropriate path for the current operating system platform.
    /// </summary>
    /// <param name="f">The FlexibleFilePath instance to convert.</param>
    /// <returns>The normalized platform-specific path, which may be relative or absolute.</returns>
    public static implicit operator LocalPath(FlexibleFilePath f) => f.GetAsLocalPath();
}