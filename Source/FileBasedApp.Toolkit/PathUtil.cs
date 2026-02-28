using System.IO.Abstractions;
using FileBasedApp.Toolkit.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides utility methods for handling and evaluating file and directory paths.
/// </summary>
public static class PathUtil
{
    private static IFileSystem _fileSystem => new FileSystem(); 
    
    internal static bool DirectoryExist(AbsolutePath path) => _fileSystem.Directory.Exists(path);
    internal static bool FileExist(AbsolutePath path) => _fileSystem.File.Exists(path);
    
    private const string EntrypointFileDirectoryPath = "EntryPointFileDirectoryPath";

    /// <summary>
    /// Gets the executing folder path of the FileBasedApp.
    /// </summary>
    /// <returns></returns>
    /// <remarks>This is achieved by using the AppContext. Note that </remarks>
    public static AbsolutePath GetExecutionFolder()
    {
        // This is relevant for FileBasedApps
        var path = AppContext.GetData(EntrypointFileDirectoryPath) as string;

        // If that value is not available we use the check below
        if (path is null)
        {
            return AbsolutePath.Create(AppDomain.CurrentDomain.BaseDirectory);
        }

        return AbsolutePath.Create(path);
    }

    /// <summary>
    /// Retrieves the root folder path based on the specified predefined root path.
    /// </summary>
    /// <param name="path">The predefined root path which specifies the base folder to resolve.</param>
    /// <returns>The resolved absolute path corresponding to the specified predefined root path.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided predefined root path is not valid.</exception>
    public static AbsolutePath GetRootFolder(this PredefinedRootPath path) => path switch
    {
        PredefinedRootPath.ExecutionFolder => GetExecutionFolder(),
        PredefinedRootPath.CurrentDirectory => GetCurrentWorkingFolder(),
        _ => throw new ArgumentOutOfRangeException(nameof(path), path, null)
    };

    /// <summary>
    /// Retrieves the current working folder path of the application.
    /// </summary>
    /// <returns>The absolute path of the current working directory.</returns>
    /// <remarks>This method provides the directory path where the application is currently running from.</remarks>
    public static AbsolutePath GetCurrentWorkingFolder()
    {
        return AbsolutePath.CurrentWorkingDirectory;
    }

    /// <summary>
    /// Analyzes a directory path candidate and evaluates its existence and validity.
    /// </summary>
    /// <param name="pathCandidate">The candidate path to analyze. Can be null.</param>
    /// <param name="rootPath">An optional root path to be used as the base for evaluation. Defaults to null.</param>
    /// <returns>
    /// An <see cref="EvaluatedPath"/> instance that contains the original path, the resolved absolute path, and a flag indicating whether the directory exists.
    /// </returns>
    public static EvaluatedPath AnalyzeDirectory(this string? pathCandidate, AbsolutePath? rootPath = null) =>
        AnalyzePath(pathCandidate, DirectoryExist, rootPath);

    /// <summary>
    /// Evaluates the specified file path candidate and determines its validity and existence.
    /// </summary>
    /// <param name="pathCandidate">The file path to analyze. Can be null or empty.</param>
    /// <param name="rootPath">An optional root path used to evaluate the file path. Defaults to null.</param>
    /// <returns>An <see cref="EvaluatedPath"/> object containing details about the evaluated file path, including its existence.</returns>
    public static EvaluatedPath AnalyzeFile(this string? pathCandidate, AbsolutePath? rootPath = null) =>
        AnalyzePath(pathCandidate, FileExist, rootPath);

    /// <summary>
    /// Tries to create a directory from a string
    /// </summary>
    /// <param name="pathCandidate"></param>
    /// <param name="evaluator"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    internal static EvaluatedPath AnalyzePath(string? pathCandidate, PathExistEvaluator evaluator, AbsolutePath? root)
    { 
        root ??= GetCurrentWorkingFolder();
      
        if (string.IsNullOrWhiteSpace(pathCandidate)) return new EvaluatedPath(pathCandidate,root,evaluator(root.Value));
        
        LocalPath testPath = new LocalPath(pathCandidate);
        AbsolutePath path;
        if (testPath.IsAbsolute)
        {
            path = new AbsolutePath(testPath);
        }
        else
        {
            path = root.Value / testPath;
        }
            
        return new EvaluatedPath(pathCandidate,path,evaluator(path));
    }
}