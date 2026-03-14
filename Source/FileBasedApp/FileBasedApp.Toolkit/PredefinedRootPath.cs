namespace FileBasedApp.Toolkit;

/// <summary>
/// Specifies predefined root paths that can be used as base directories for file or directory operations.
/// </summary>
public enum PredefinedRootPath
{
    /// <summary>
    /// This will return the folder os the file based type cs file
    /// </summary>
    ExecutionFolder,
    /// <summary>
    /// The root path will be that of the current working directory
    /// where this is called from
    /// </summary>
    CurrentDirectory,
    
}