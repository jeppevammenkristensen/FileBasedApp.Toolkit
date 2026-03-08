using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Runtime.Serialization.Formatters;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides utility methods for handling and evaluating file and directory paths.
/// </summary>
public class PathUtil : IStaticFileSystemSetter
{
    private static IFileSystem DefaultFileSystem => new FileSystem();

    private static IFileSystem _fileSystem = DefaultFileSystem;
        
        
    static IFileSystem IStaticFileSystemSetter.GetDefault()
    {
        return new FileSystem();
    }

    static IFileSystem IStaticFileSystemSetter.GetFileSystem()
    {
        return _fileSystem;
    }

    static void IStaticFileSystemSetter.SetFileSystem(IFileSystem fileSystem)
    {
        SetFileSystem(fileSystem);
    }

    /// <summary>
    /// For unit test scenarios 
    /// </summary>
    /// <param name="fileSystem"></param>
    internal static void SetFileSystem(IFileSystem fileSystem) => _fileSystem = fileSystem;
    
    internal static bool DirectoryExist(AbsolutePath path) => path.DirectoryExists(_fileSystem);
    internal static bool FileExist(AbsolutePath path) => path.FileExists(_fileSystem);
    
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

        // If that value is not available we use the check below. 
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
    public static AbsolutePath GetRootFolder(PredefinedRootPath path) => path switch
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
    /// <param name="rootPaths"> optional root paths to be used as the base for evaluation. If none are defined the current working directory is used.</param>
    /// <returns>
    /// An <see cref="EvaluatedPath"/> instance that contains the original path, the resolved absolute path, and a flag indicating whether the directory exists.
    /// </returns>
    public static IEvaluatedPath AnalyzeDirectory(string? pathCandidate, params AbsolutePath[] rootPaths) =>
        AnalyzePath(pathCandidate, DirectoryExist, rootPaths);

    /// <summary>
    /// Evaluates the specified file path candidate and determines its validity and existence.
    /// </summary>
    /// <param name="pathCandidate">The file path to analyze. Can be null or empty.</param>
    /// <param name="rootPaths">An optional root path used to evaluate the file path. Defaults to null.</param>
    /// <returns>An <see cref="EvaluatedPath"/> object containing details about the evaluated file path, including its existence.</returns>
    public static IEvaluatedPath AnalyzeFile(string? pathCandidate, params AbsolutePath[] rootPaths) =>
        AnalyzePath(pathCandidate, FileExist, rootPaths);

    /// <summary>
    /// Tries to create a directory from a string
    /// </summary>
    /// <param name="pathCandidate"></param>
    /// <param name="evaluator"></param>
    /// <param name="roots"></param>
    /// <returns></returns>
    internal static IEvaluatedPath AnalyzePath(string? pathCandidate,
        PathExistEvaluator evaluator,
        IReadOnlyList<AbsolutePath> roots)
    {
        Queue<AbsolutePath> rootsQueue = roots.Count == 0 ? new([GetCurrentWorkingFolder()]) : new([..roots]);
        ImmutableArray<EvaluatedPath> evaluatedPaths = ImmutableArray.Create<EvaluatedPath>(); 
        
        while (rootsQueue.Count > 0)
        {
            var rootCandidate = rootsQueue.Dequeue();

            // If the path candidate is empty we return the root candidate
            if (string.IsNullOrWhiteSpace(pathCandidate))
            {
                evaluatedPaths =
                    evaluatedPaths.Add(new EvaluatedPath(pathCandidate, rootCandidate, evaluator(rootCandidate)));
            }
            else
            {
                LocalPath testPath = new LocalPath(pathCandidate);
                AbsolutePath path;
                if (testPath.IsAbsolute)
                {
                    path = new AbsolutePath(testPath);
                }
                else
                {
                    path = rootCandidate / testPath;
                }

                evaluatedPaths = evaluatedPaths.Add(new EvaluatedPath(pathCandidate, path, evaluator(path)));
            }
        }
        
        return new EvaluatedPaths(evaluatedPaths);
        
    }

}