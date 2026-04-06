using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using TruePath;

namespace FileBasedApp.Toolkit;

public static class Parse
{
    /// <summary>
    /// Attempts to parse the specified string value into the target type using the specified format provider.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed value if the conversion succeeded, or null if the conversion failed.</param>
    /// <typeparam name="T">The target type that implements IParsable. Must be parsable from a string representation.</typeparam>
    /// <return>True if the value was successfully parsed; otherwise, false.</return>
    public static bool TryParseTo<T>(this string? value,
        [MaybeNullWhen(returnValue:false)] out T result) where T : IParsable<T>
    {
        return TryParseTo(value, null, out result);
    }

    /// <summary>
    /// Safely parses the specified string value into the target struct type, returning null if parsing fails.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="formatProvider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? SafeParseToStruct<T>(string? value, IFormatProvider? formatProvider = null)
        where T : struct, IParsable<T>
    {
        return TryParseTo(value, formatProvider, out T result) ? result : null;
    }

    /// <summary>
    /// Safely parses the specified string value into the target reference type, returning null if parsing fails.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <typeparam name="T">The target reference type that implements IParsable. Must be a class that is parsable from a string representation.</typeparam>
    /// <return>The parsed value if the conversion succeeded; otherwise, null.</return>
    public static T? SafeParseToClass<T>(string? value, IFormatProvider? formatProvider = null)
        where T : class, IParsable<T>
    {
        return TryParseTo(value, formatProvider, out T result) ? result : null;
    }

    /// <summary>
    /// Attempts to parse the specified string value into the target type using the specified format provider.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the parsed value if the conversion succeeded, or null if the conversion failed.</param>
    /// <typeparam name="T">The target type that implements IParsable. Must be parsable from a string representation.</typeparam>
    /// <return>True if the value was successfully parsed; otherwise, false.</return>
    public static bool TryParseTo<T>(this string? value, IFormatProvider? formatProvider,
        [MaybeNullWhen(returnValue:false)] out T result) where T : IParsable<T>
    {
        return T.TryParse(value, formatProvider, out result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="formatProvider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RequiredParse<T>(this string? value, IFormatProvider? formatProvider = null) where T : IParsable<T> 
    {
        if (T.TryParse(value, formatProvider, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"Unable to parse the value '{value}' to type {typeof(T).FullName}.");
    }
        
}

/// <summary>
/// Provides utility methods for handling and evaluating file and directory paths.
/// </summary>
public class PathUtil : IStaticFileSystemSetter
{
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
    internal static void SetFileSystem(IFileSystem fileSystem) => _fileSystem = fileSystem;
    
    internal static bool DirectoryExist(AbsolutePath path) => path.DirectoryExists(_fileSystem);
    internal static bool FileExist(AbsolutePath path) => path.FileExists(_fileSystem);
    
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