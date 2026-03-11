using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Utils and extensions for IO operation revolving around IO
/// </summary>
public static class IO
{
    /// <summary>
    /// Retrieves the absolute path of the specified special folder provided by the operating system.
    /// </summary>
    /// <param name="folder">The special folder whose absolute path is to be retrieved.</param>
    /// <returns>The absolute path corresponding to the specified special folder.</returns>
    public static AbsolutePath GetSpecialFolder(this Environment.SpecialFolder folder)
    {
        return Environment.GetFolderPath(folder).AsRequiredAbsolutePath();
    }

    /// <summary>
    /// Represents the default maximum depth value used for file or folder traversal operations in the IO utilities.
    /// This constant is primarily utilized by methods like <see cref="FindRequiredParent"/> and <see cref="GetAncestors"/>
    /// to limit the depth of recursive or iterative operations, preventing infinite loops or unnecessary traversal.
    /// </summary>
    public const int DefaultMaxDepth = 20;

    /// <summary>
    /// Finds the nearest ancestor directory of the given folder that matches the specified predicate.
    /// </summary>
    /// <param name="folder">The folder to start searching from.</param>
    /// <param name="predicate">A function to test each ancestor directory for a match.</param>
    /// <param name="maxDepth">The maximum number of levels to search upward. Defaults to <see cref="DefaultMaxDepth"/>.</param>
    /// <returns>The absolute path of the first matching ancestor directory.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no matching ancestor directory is found within the specified depth.
    /// </exception>
    public static AbsolutePath FindRequiredParent(this AbsolutePath folder, Func<AbsolutePath, bool> predicate,
        int maxDepth = DefaultMaxDepth)
    {
        return folder.FindAncestorOrNull(false, predicate, maxDepth) ??
               throw new InvalidOperationException($"Could not find parent folder for path {folder}");
    }

    /// <summary>
    /// Attempts to find a parent folder of the specified folder that satisfies the given predicate.
    /// Returns null if no matching parent folder is found.
    /// </summary>
    /// <param name="folder">The starting folder from which the search for a parent folder begins.</param>
    /// <param name="predicate">A function that defines the condition to be met by the parent folder.</param>
    /// <param name="maxDepth">
    /// The maximum number of folder levels to traverse upwards in the hierarchy.
    /// Defaults to <see cref="DefaultMaxDepth"/>.
    /// </param>
    /// <returns>
    /// The parent folder, if found, that matches the specified predicate,
    /// or null if no such folder is found.
    /// </returns>
    public static AbsolutePath? FindParentOrNull(this AbsolutePath folder, Func<AbsolutePath, bool> predicate,
        int maxDepth = DefaultMaxDepth)
    {
        return folder.FindAncestorOrNull(false, predicate, maxDepth) ??
               throw new InvalidOperationException($"Could not find parent folder for path {folder}");
    }

    /// <summary>
    /// Retrieves a single matching path from the source collection based on the specified predicate.
    /// Throws an exception if no match or multiple matches are found.
    /// </summary>
    /// <param name="source">The collection of absolute paths to search through.</param>
    /// <param name="predicate">The condition to evaluate for selecting a path.</param>
    /// <param name="noMatchErrorMessage">
    /// The error message included in the exception if no match is found. Default is "No single path found".
    /// </param>
    /// <param name="multipleMatchesError">
    /// The error message included in the exception if multiple matches are found. Default is "Found more than 1 match".
    /// </param>
    /// <returns>The single matching path that satisfies the predicate.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no path matches the predicate or if more than one path matches.
    /// </exception>
    public static AbsolutePath GetSingle(this IEnumerable<AbsolutePath> source, Func<AbsolutePath, bool> predicate,
        string? noMatchErrorMessage = null, string? multipleMatchesError = null)
    {
        noMatchErrorMessage ??= "No single path found";
        multipleMatchesError ??= "Found more than 1 match";
        var candidates = source.Where(predicate).ToList();

        return candidates switch
        {
            { Count: 1} => candidates[0],
            { Count: > 1 } => throw new InvalidOperationException(multipleMatchesError),
            _ => throw new InvalidOperationException(noMatchErrorMessage)
        };

    }
    
    /// <summary>
    /// Traverses the parents of the given path based on the <see cref="Predicate{T}"/>
    /// </summary>
    /// <param name="path">The pass to traverse up</param>
    /// <param name="includeSelf">The predicate should also be applied to the original path</param>
    /// <param name="predicate">The search predicate</param>
    /// <param name="maxDepth">The max depth to traverse up before skipping travering (to avoid endless loop) default is <see cref="DefaultMaxDepth"/></param>
    /// <returns></returns>
    internal static AbsolutePath? FindAncestorOrNull(this AbsolutePath path, bool includeSelf, Func<AbsolutePath, bool> predicate,
        int maxDepth = DefaultMaxDepth)
    {
        return path
            .GetAncestors(includeSelf, maxDepth)
            .FirstOrDefault(predicate);
    }
    
    /// <summary>
    /// Returns the path or a nullable 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static AbsolutePath? AsAbsolutePath(this string? path) => path is null ? null : AbsolutePath.Create(path);
    
    /// <summary>
    /// Returns a an non nullable path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static AbsolutePath AsRequiredAbsolutePath(this string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return AbsolutePath.Create(path);
    }

    /// <summary>
    /// Tries to convert the provided string to an absolute path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static AbsolutePath ToRequired(this AbsolutePath? path) =>
        path ?? throw new ArgumentNullException(nameof(path));
    
    /// <summary>
    /// Returns the path or a nullable
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static AbsolutePath? ToNullable(this AbsolutePath path) => path;

    /// <summary>
    /// Converts the provided path, given as a read-only span of characters, to an absolute path.
    /// </summary>
    /// <param name="path">The path to be converted, represented as a read-only span of characters.</param>
    /// <returns>An instance of <see cref="AbsolutePath"/> representing the absolute path, or null if the conversion fails.</returns>
    public static AbsolutePath? AsAbsolutePath(this ReadOnlySpan<char> path) => AbsolutePath.Create(path.ToString());

    /// <summary>
    /// Retrieves all ancestor paths for the specified path, optionally including the path itself, up to a defined depth.
    /// </summary>
    /// <param name="source">The starting path from which to retrieve ancestor paths.</param>
    /// <param name="includeSelf">A boolean value indicating whether to include the source path in the result.</param>
    /// <param name="maxDepth">The maximum depth to traverse when retrieving ancestor paths. Defaults to <see cref="DefaultMaxDepth"/>.</param>
    /// <returns>An enumerable sequence of ancestor paths ordered from the closest to the farthest.</returns>
    public static IEnumerable<AbsolutePath> GetAncestors(this AbsolutePath source, bool includeSelf,
        int maxDepth = DefaultMaxDepth)
    {
        maxDepth = maxDepth < 1 ? DefaultMaxDepth : maxDepth;
        
        if (includeSelf) yield return source;
        int currentDepth = 0;
        var currentPath = source;

        while (currentDepth < maxDepth)
        {
            if (currentPath.Parent is null) yield break;
            currentPath = currentPath.Parent.Value;
            yield return currentPath;
            currentDepth++;
        }
    }
}