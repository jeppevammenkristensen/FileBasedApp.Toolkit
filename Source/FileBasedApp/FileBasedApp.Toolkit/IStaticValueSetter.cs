namespace FileBasedApp.Toolkit;

internal interface IStaticValueSetter<T>
{
    static abstract T GetDefault();
    
    static abstract T GetFileSystem();
    static abstract void SetFileSystem(T fileSystem);
}