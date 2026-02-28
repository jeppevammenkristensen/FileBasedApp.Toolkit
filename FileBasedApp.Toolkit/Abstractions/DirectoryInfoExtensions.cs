using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit.Abstractions;

public static class DirectoryInfoExtensions
{
    /// <inheritdoc cref="DirectoryInfo.CreateSubdirectory(string)" />
    public static IDirectoryInfo CreateSubdirectory(this System.IO.Abstractions.IDirectoryInfo directoryInfo,
        AbsolutePath path)
    {
        var result = directoryInfo.CreateSubdirectory(path.Value);
        return result;
    }
}