using System.IO.Abstractions;

namespace FileBasedApp.Toolkit;

internal class FileSystemSetter<T> : IDisposable where T : IStaticFileSystemSetter
{
    public FileSystemSetter(IFileSystem fileSystem)
    {
        T.SetFileSystem(fileSystem);
    }
    
    public void Dispose()
    {
        T.SetFileSystem(T.GetDefault());
    }
}