using System.IO.Abstractions;
using System.Text.RegularExpressions;
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
    /// Traverses the parents of the given path based on the <see cref="Predicate{T}"/>
    /// </summary>
    /// <param name="path">The pass to traverse up</param>
    /// <param name="includeSelf">The predicate should also be applied to the original path</param>
    /// <param name="predicate">The search predicate</param>
    /// <param name="maxDepth">The max depth to traverse up before skipping travering (to avoid endless loop) default is <see cref="DefaultMaxDepth"/></param>
    /// <returns></returns>
    internal static AbsolutePath? FindAncestorOrNull(this AbsolutePath path, bool includeSelf,
        Func<AbsolutePath, bool> predicate,
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


    /// <summary>
    /// Enumerates all directories within the specified source path that match the given search pattern, including subdirectories at all levels.
    /// </summary>
    /// <param name="source">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard characters.</param>
    /// <param name="fileSystem">The file system abstraction to use for the operation. If null, the default file system is used.</param>
    /// <returns>An enumerable collection of absolute paths representing all directories that match the search pattern within the source directory and its subdirectories.</returns>
    /// <remarks>This is a convenience method for all directories matching the pattern</remarks> 
    public static IEnumerable<AbsolutePath> EnumerateAllDirectories(this AbsolutePath source, string searchPattern,
        IFileSystem? fileSystem = null)
        => source.EnumerateDirectories(searchPattern, SearchOption.AllDirectories, fileSystem);

    /// <summary>
    /// Filters a sequence of file paths by applying a predicate to the content stream of each file.
    /// </summary>
    /// <param name="source">The sequence of absolute paths to files to be filtered.</param>
    /// <param name="predicate">A function that examines the content stream of a file and returns true if the file should be included in the result.</param>
    /// <param name="fileSystem">An optional file system abstraction to use for file operations. If null, the default file system is used.</param>
    /// <returns>A sequence of absolute paths to files for which the predicate returned true.</returns>
    public static IEnumerable<AbsolutePath> FindInFiles(this IEnumerable<AbsolutePath> source,
        Func<Stream, bool> predicate, IFileSystem? fileSystem = null)
    {
        foreach (var item in source)
        {
            using var fileSystemStream = item.OpenRead(fileSystem);
            if (predicate(fileSystemStream))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Replaces the contents of a file with the specified text.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="converter"></param>
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    public static AbsolutePath Replace(this AbsolutePath path, Func<string, string> converter,
        IFileSystem? fileSystem = null)
    {
        var existing = path.ReadAllText(fileSystem);
        var newText = converter(existing);
        if (existing.Equals(newText)) return path;
        path.WriteAllText(newText, fileSystem);
        return path;
    }
    
    
    /// <summary>
    /// Filters a collection of file paths to only those whose contents match the specified regular expression pattern.
    /// </summary>
    /// <param name="source">The collection of file paths to search through.</param>
    /// <param name="regex">The regular expression pattern to match against the contents of each file.</param>
    /// <param name="searchStrategy"></param>
    /// <param name="fileSystem">The file system abstraction to use for file operations. If null, the default file system is used.</param>
    /// <returns>A collection of file paths whose contents contain at least one line matching the regular expression pattern.</returns>
    public static IEnumerable<AbsolutePath> FindInFiles(this IEnumerable<AbsolutePath> source, Regex regex, FileSearchStrategy searchStrategy = FileSearchStrategy.ByLine,
        IFileSystem? fileSystem = null)
    {
        return source.FindInFiles((Func<Stream, bool>) ContainsRegex, fileSystem);

        bool ContainsRegex(Stream s)
        {
            using var streamReader = new StreamReader(s);

            if (searchStrategy == FileSearchStrategy.AllText)
            {
                return regex.IsMatch(streamReader.ReadToEnd());
            }
            else if (searchStrategy == FileSearchStrategy.ByLine)
            {
                var currentLine = streamReader.ReadLine();

                while (currentLine != null)
                {
                    if (regex.IsMatch(currentLine))
                    {
                        return true;
                    }

                    currentLine = streamReader.ReadLine();
                }    
            }
            
            return false;
        }
    }

/// <summary>
    /// Retrieves all subdirectories within the specified directory that match the given search pattern, including all nested subdirectories.
    /// </summary>
    /// <param name="source">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match against the names of subdirectories.</param>
    /// <param name="fileSystem">The file system abstraction to use for the operation. If null, the default file system is used.</param>
    /// <returns>An enumerable collection of absolute paths representing all matching subdirectories found within the source directory and its subdirectories.</returns>
    /// <remarks>This is a convenience method for all directories matching the pattern</remarks>
    public static AbsolutePath[] GetAllDirectories(this AbsolutePath source, string searchPattern,
        IFileSystem? fileSystem = null)
        => source.GetDirectories(searchPattern, SearchOption.AllDirectories, fileSystem);
    
    /// <summary>
    /// Enumerates all files in the specified directory and its subdirectories that match the given search pattern.
    /// </summary>
    /// <param name="source">The directory path to search for files.</param>
    /// <param name="searchPattern">The search pattern to match against the names of files. Supports wildcards such as * and ?.</param>
    /// <param name="fileSystem">The file system abstraction to use for enumeration. If null, the default file system is used.</param>
    /// <returns>An enumerable collection of absolute paths representing all files that match the search pattern in the directory and its subdirectories.</returns>
    /// <remarks>This is a convenience method for all files matching the pattern</remarks>
    public static IEnumerable<AbsolutePath> EnumerateAllFiles(this AbsolutePath source, string searchPattern,
        IFileSystem? fileSystem = null)
        => source.EnumerateFiles(searchPattern, SearchOption.AllDirectories, fileSystem);

    /// <summary>
    /// Retrieves all files from the specified directory and its subdirectories that match the given search pattern.
    /// </summary>
    /// <param name="source">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern to match against the names of files.</param>
    /// <param name="fileSystem">The file system abstraction to use for file operations. If null, the default file system is used.</param>
    /// <returns>An array of absolute paths representing all files found that match the search pattern.</returns>
    /// <remarks>This is a convenience method for returning all files matching the pattern</remarks>
    public static AbsolutePath[] GetAllFiles(this AbsolutePath source, string searchPattern,
        IFileSystem? fileSystem = null)
        => source.GetFiles(searchPattern, SearchOption.AllDirectories, fileSystem);

    /// <summary>
    /// Attempts to delete the specified directory and its contents, suppressing any exceptions that occur during deletion.
    /// </summary>
    /// <param name="path">The absolute path of the directory to delete.</param>
    /// <param name="exceptionHandler">An optional callback action to handle any exceptions that occur during the deletion operation.</param>
    /// <param name="fileSystem">An optional file system abstraction to use for the operation. If null, the default file system is used.</param>
    public static void SafeDeleteDirectory(this AbsolutePath path, Action<Exception>? exceptionHandler = null, IFileSystem? fileSystem = null)
    {
        try
        {
            if (path.DirectoryExists(fileSystem))
            {
                path.DirectoryDelete(true, fileSystem);    
            }
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
        }
    }

    /// <summary>
    /// Defines the strategy for searching file content with a regular expression pattern.
    /// ByLine checks each line individually, while AllText searches the entire file content as a single string.
    /// </summary>
    public enum FileSearchStrategy
    {
        /// <summary>
        /// A filter is applied by reading every line one by one
        /// </summary>
        /// <remarks>Can give performance boost if the files being searched are expected to be vary large</remarks>
        ByLine,
        /// <summary>
        /// All text is loaded in. If the filter used expects to work on multiplelines this is the right approach
        /// </summary>
        /// <remarks>God for medium sized files</remarks>
        AllText
    }
}