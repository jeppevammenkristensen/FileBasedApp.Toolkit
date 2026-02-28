using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit.Abstractions;

public static class FileSystemInfoExtensions
{
#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="FileSystemInfo.CreateAsSymbolicLink(string)" />
    public static void CreateAsSymbolicLink(this IFileSystemInfo fileSystemInfo, AbsolutePath pathToTarget)
    {
        fileSystemInfo.CreateAsSymbolicLink(pathToTarget.Value);
    }
#endif
}


