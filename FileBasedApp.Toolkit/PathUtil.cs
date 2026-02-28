using TruePath;

namespace FileBasedApp.Toolkit;

public static class PathUtil
{
    internal static bool DirectoryExist(AbsolutePath path) => Directory.Exists(path.ToString());
    internal static bool FileExist(AbsolutePath path) => File.Exists(path.ToString());
    
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

    public static AbsolutePath GetRootFolder(this PredefinedRootPath path) => path switch
    {
        PredefinedRootPath.ExecutionFolder => GetExecutionFolder(),
        PredefinedRootPath.CurrentDirectory => GetCurrentWorkingFolder(),
        _ => throw new ArgumentOutOfRangeException(nameof(path), path, null)
    };

    public static AbsolutePath GetCurrentWorkingFolder()
    {
        return AbsolutePath.CurrentWorkingDirectory;
    }

    public static EvaluatedPath AnalyzeDirectory(this string? pathCandidate, AbsolutePath? rootPath = null) => AnalyzePath(pathCandidate, DirectoryExist, rootPath);
    
    public static EvaluatedPath AnalyzeFile(this string? pathCandidate, AbsolutePath? rootPath = null) => AnalyzePath(pathCandidate, FileExist, rootPath);

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