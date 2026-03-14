using System.IO.Abstractions;

namespace FileBasedApp.Toolkit;

internal interface IStaticValueSetter<T>
{
    static abstract T GetDefault();
    
    static abstract T GetFileSystem();
    static abstract void SetFileSystem(T fileSystem);
}

internal interface IStaticFileSystemSetter : IStaticValueSetter<IFileSystem>
{
    // static abstract IFileSystem GetDefault();
    //
    // static abstract IFileSystem GetFileSystem();
    // static abstract void SetFileSystem(IFileSystem fileSystem);
}