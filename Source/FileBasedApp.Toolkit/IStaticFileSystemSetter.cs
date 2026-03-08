using System.IO.Abstractions;

namespace FileBasedApp.Toolkit;

internal interface IStaticFileSystemSetter
{
    static abstract IFileSystem GetDefault();
    
    static abstract IFileSystem GetFileSystem();
    static abstract void SetFileSystem(IFileSystem fileSystem);

}