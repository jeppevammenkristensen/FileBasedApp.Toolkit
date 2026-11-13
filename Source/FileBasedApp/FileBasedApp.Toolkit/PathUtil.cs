using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides utility methods for handling and evaluating file and directory paths.
/// </summary>
public class PathUtil : IStaticFileSystemSetter
{
    /// <summary>
    /// Tries to get a directory from a string
    /// </summary>
    /// <param name="candidatePath">The candidate to evaluate</param>
    /// <param name="root">The path to use as root (in case the candidatePath is relative or empty)</param>
    /// <param name="allowEmpty">Allows the candidatePath to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exist</param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePath))] string? param = null)
    {
        return TryGetDirectory(candidatePath, allowEmpty, shouldExist, root.GetRootFolder(), param);
    }

    /// <summary>
    /// Resolves directory candidates using a predefined root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to the root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="root">The predefined root used to resolve relative or empty candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    public static AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetDirectories(candidatePaths, allowEmpty, shouldExist, paramName, root.GetRootFolder());
    }

    /// <summary>
    /// Resolves directory candidates using an absolute root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to the root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="root">The root used to resolve relative or empty candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    public static AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        AbsolutePath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetDirectories(candidatePaths, allowEmpty, shouldExist, paramName, root);
    }

    /// <summary>
    /// Tries to get a directory from a string
    /// </summary>
    /// <param name="candidatePath">The candidate to evaluate</param>
    /// <param name="allowEmpty">Allows the candidatePath to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exist</param>
    /// <param name="rootPaths">The paths to use as roots (in case the candidatePath is relative or empty)</param>
    /// <param name="paramName">Used in the exception thrown</param>
    /// <returns>The absolute path</returns>
    /// <remarks>If no paths are added. The current working directory is used as default root</remarks>
    /// <exception cref="InvalidOperationException">Throw an exception that can be caught by the do validate method</exception>
    public static AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null, params AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
#if !NET11_0_OR_GREATER
        (AbsolutePath path, string? errorMessage) = result.GetPath(shouldExist, allowEmpty);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
#else
        return result.GetPath(shouldExist, allowEmpty).GetPathOrThrow(s => new InvalidOperationException(s));
#endif
    }

    /// <summary>
    /// Resolves directory paths against the supplied roots and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The candidate directory paths. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to a root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <param name="rootPaths">The roots used to resolve relative or empty candidates; defaults to the current working directory.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="candidatePaths"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when directory validation fails; errors for multiple candidates are aggregated.</exception>
    public static AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null, params AbsolutePath[] rootPaths)
    {
        ArgumentNullException.ThrowIfNull(candidatePaths);

        if (candidatePaths.Length == 0)
        {
            return [TryGetDirectory(null, allowEmpty, shouldExist, paramName, rootPaths)];
        }

        if (candidatePaths.Length == 1)
        {
            return [TryGetDirectory(candidatePaths[0], allowEmpty, shouldExist, paramName, rootPaths)];
        }

        List<string> errors = [];
        List<AbsolutePath> paths = [];

        foreach (var candidatePath in candidatePaths)
        {
            var result = candidatePath.AnalyzeDirectory(rootPaths);
#if !NET11_0_OR_GREATER
            var (path, errorMessage) = result.GetPath(shouldExist, allowEmpty);
            if (errorMessage != null)
            {
                errors.Add(errorMessage);
            }
            else
            {
                paths.Add(path);
            }
#else
            var pathOrError = result.GetPath(shouldExist, allowEmpty);
            if (pathOrError is AbsolutePath path)
            {
                paths.Add(path);
            }
            else if (pathOrError is string error)
            {
                errors.Add(error);
            }
#endif
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"{paramName}: {errors.Count} directories: {string.Join(", ", errors)}");
        }

        return paths.ToArray();
    }

    /// <summary>
    /// Tries to get a directory from the candidate path
    /// </summary>
    /// <param name="candidatePath">The candidate path</param>
    /// <param name="root">The root folder to use in case the candidatePath is relative</param>
    /// <param name="allowEmpty">Allows the candidate to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exists</param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist, AbsolutePath root,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        return TryGetDirectory(candidatePath, allowEmpty, shouldExist, paramName, root);
    }

    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="root">The <see cref="AbsolutePath"/> representing the root path to use when the candidate is relative</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    public static AbsolutePath TryGetFile(string candidatePath, bool shouldExist, AbsolutePath root,
        [CallerArgumentExpression(nameof(candidatePath))]
        string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);

        return TryGetFile(candidatePath, shouldExist, paramName, root);
    }

    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="roots">The <see cref="AbsolutePath"/> representing the root path to use when the candidate is relative</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    public static AbsolutePath[] TryGetFiles(string[] candidatePath, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null,
        params AbsolutePath[] roots)
    {
        ArgumentNullException.ThrowIfNull(candidatePath);

        if (candidatePath.Length == 0)
        {
            throw new ArgumentException("Paths cannot be empty", nameof(candidatePath));
        }
        else if (candidatePath.Length == 1)
        {
            return [TryGetFile(candidatePath[0], shouldExist, paramName, roots)];
        }

        List<string> errors = [];
        List<AbsolutePath> paths = [];

        foreach (var s in candidatePath)
        {
#if !NET11_0_OR_GREATER
            var (path, errorMessage) = s.AnalyzeFile(roots).GetPath(shouldExist, false);
            if (errorMessage != null)
            {
                errors.Add(errorMessage);
            }
            else
            {
                paths.Add(path);
            }

#else
            var result = TryGetFileAsPathOrError(s, shouldExist);
            if (result is AbsolutePath path)
            {
                paths.Add(path);
            }
            else if (result is string error)
            {
                errors.Add(error);
            }
#endif
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"{errors.Count} files {string.Join(",", errors)}");
        }

        return paths.ToArray();
    }

    /// <summary>
    /// Resolves file candidates using a predefined root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The non-empty array of file candidates.</param>
    /// <param name="shouldExist">Whether each resolved file must exist.</param>
    /// <param name="root">The predefined root used to resolve relative candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute file paths in candidate order.</returns>
    public static AbsolutePath[] TryGetFiles(string[] candidatePaths, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetFiles(candidatePaths, shouldExist, paramName, root.GetRootFolder());
    }

    /// <summary>
    /// Resolves file candidates using an absolute root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The non-empty array of file candidates.</param>
    /// <param name="shouldExist">Whether each resolved file must exist.</param>
    /// <param name="root">The root used to resolve relative candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute file paths in candidate order.</returns>
    public static AbsolutePath[] TryGetFiles(string[] candidatePaths, bool shouldExist,
        AbsolutePath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetFiles(candidatePaths, shouldExist, paramName, root);
    }

    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="roots">The <see cref="AbsolutePath"/> representing the root path to use when the candidate is relative</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    public static AbsolutePath TryGetFile(string candidatePath, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null,
        params AbsolutePath[] roots)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"{paramName}: Candidate path cannot be empty");
        }

#if !NET11_0_OR_GREATER
        var (path, errorMessage) = candidatePath.AnalyzeFile(roots).GetPath(shouldExist, false);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
#else
        return candidatePath.AnalyzeFile()
            .GetPath(shouldExist, false).GetPathOrThrow(s => new InvalidOperationException($"{paramName}: {s}"));
#endif
    }

#if NET11_0_OR_GREATER
    /// <summary>
    /// Resolves a non-empty file candidate and returns either its absolute path or a validation error.
    /// </summary>
    /// <param name="candidatePath">The file path to resolve using the default roots.</param>
    /// <param name="shouldExist">Whether the resolved file must exist.</param>
    /// <returns>The resolved file path or an error describing why validation failed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="candidatePath"/> is null, empty, or whitespace.</exception>
    private static PathOrError TryGetFileAsPathOrError(string candidatePath, bool shouldExist)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"Candidate path cannot be empty");
        }

        return candidatePath.AnalyzeFile()
            .GetPath(shouldExist, false);
    }

    /// <summary>
    /// Resolves a directory candidate against the supplied roots, throwing if validation fails.
    /// </summary>
    /// <param name="candidatePath">The directory path to resolve, which may be empty when allowed.</param>
    /// <param name="shouldExist">Whether the resolved directory must exist.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to a root directory.</param>
    /// <param name="rootPaths">The roots used to resolve relative or empty candidates.</param>
    /// <returns>The resolved absolute directory path wrapped as a <see cref="PathOrError"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when directory validation fails.</exception>
    private static PathOrError TryGetDirectoryAsPathOrError(string? candidatePath, bool shouldExist, bool allowEmpty, AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
        return result.GetPath(shouldExist, allowEmpty).GetPathOrThrow(s => new InvalidOperationException(s));
    }

#endif

    private static IFileSystem DefaultFileSystem => new FileSystem();

    private static IFileSystem _fileSystem = DefaultFileSystem;
        
        
    static IFileSystem IStaticValueSetter<IFileSystem>.GetDefault()
    {
        return new FileSystem();
    }

    static IFileSystem IStaticValueSetter<IFileSystem>.GetFileSystem()
    {
        return _fileSystem;
    }

    static void IStaticValueSetter<IFileSystem>.SetFileSystem(IFileSystem fileSystem)
    {
        SetFileSystem(fileSystem);
    }

    /// <summary>
    /// For unit test scenarios 
    /// </summary>
    /// <param name="fileSystem"></param>
    private static void SetFileSystem(IFileSystem fileSystem) => _fileSystem = fileSystem;

    private static bool DirectoryExist(AbsolutePath path) => path.DirectoryExists(_fileSystem);
    private static bool FileExist(AbsolutePath path) => path.FileExists(_fileSystem);
    
    private const string EntrypointFileDirectoryPath = "EntryPointFileDirectoryPath";
    private const string EntrypointFilePath = "EntryPointFilePath";


    /// <summary>
    /// Gets the executing file path of the FileBasedApp.
    /// </summary>
    /// <returns>The absolute path to the entrypoint file.</returns>
    /// <remarks>This is achieved by using the AppContext, with a fallback to the entry assembly location.</remarks>
    public static AbsolutePath GetExecutionFile()
    {
        var path = AppContext.GetData(EntrypointFilePath) as string;

        if (path is null)
        {
            var assemblyLocation = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                throw new InvalidOperationException("Unable to determine the execution file path.");
            }
            return AbsolutePath.Create(assemblyLocation);
        }

        return AbsolutePath.Create(path);
    }
    
    /// <summary>
    /// Gets the executing folder path of the FileBasedApp.
    /// </summary>
    /// <returns></returns>
    /// <remarks>This is achieved by using the AppContext. Note that </remarks>
    public static AbsolutePath GetExecutionFolder()
    {
        // This is relevant for FileBasedApps

        // If that value is not available we use the check below. 
        return AbsolutePath.Create(AppContext.GetData(EntrypointFileDirectoryPath) as string ?? AppDomain.CurrentDomain.BaseDirectory);
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
