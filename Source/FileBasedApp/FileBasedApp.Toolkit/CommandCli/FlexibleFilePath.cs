using System.Runtime.InteropServices;
using TruePath;

namespace FileBasedApp.Toolkit.CommandCli;

/// <summary>
/// Represents a flexible file path that can be used across different operating systems.
/// and in a json file
/// </summary>
/// <param name="Windows">The windows path</param>
/// <param name="Unix">The unix path</param>
public record FlexibleFilePath(string Windows, string Unix)
{
    /// <summary>
    /// return a path based on the os platform. Windows or non windows
    /// </summary>
    /// <returns></returns>
    public LocalPath GetAsLocalPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return AbsolutePath.Create(Windows);
        }
        else
        {
            return AbsolutePath.Create(Unix);
        }
    }

    /// <summary>
    /// Implicitly converts a FlexibleFilePath to a LocalPath by selecting the appropriate path for the current operating system platform.
    /// </summary>
    /// <param name="f">The FlexibleFilePath instance to convert.</param>
    /// <returns>A LocalPath representing the platform-specific absolute path.</returns>
    public static implicit operator LocalPath(FlexibleFilePath f) => f.GetAsLocalPath();
}