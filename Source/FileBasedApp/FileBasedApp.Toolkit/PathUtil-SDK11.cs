#if NET11_0_OR_GREATER
using TruePath;

namespace FileBasedApp.Toolkit;

public partial class PathUtil
{
    /// <summary>
    /// Resolves a directory candidate and validates its existence when requested.
    /// </summary>
    /// <param name="candidatePath">The directory candidate, which may be relative or absolute.</param>
    /// <param name="allowEmpty">Whether a null, empty, or whitespace candidate may resolve to a root.</param>
    /// <param name="shouldExist">Whether the resolved directory must exist.</param>
    /// <param name="rootPaths">The roots to search in order. Defaults to the current working directory.</param>
    /// <returns>The resolved absolute path or a validation error.</returns>
    public static Result<AbsolutePath, ErrorString> GetDirectoryResult(string? candidatePath, bool allowEmpty,
        bool shouldExist,
        params AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
        return result.GetPath(shouldExist, allowEmpty);
    }

    /// <summary>
    /// Resolves a directory candidate against a predefined root.
    /// </summary>
    /// <param name="candidatePath">The directory candidate, which may be relative or absolute.</param>
    /// <param name="allowEmpty">Whether a null, empty, or whitespace candidate may resolve to the root.</param>
    /// <param name="shouldExist">Whether the resolved directory must exist.</param>
    /// <param name="root">The predefined root for relative or allowed empty candidates.</param>
    /// <returns>The resolved absolute path or a validation error.</returns>
    public static Result<AbsolutePath, ErrorString> GetDirectoryResult(string? candidatePath, bool allowEmpty,
        bool shouldExist,
        PredefinedRootPath root) =>
        GetDirectoryResult(candidatePath, allowEmpty, shouldExist, root.GetRootFolder());

    /// <summary>
    /// Resolves a batch of directory candidates and combines any validation errors.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as one empty candidate.</param>
    /// <param name="allowEmpty">Whether a null, empty, or whitespace candidate may resolve to a root.</param>
    /// <param name="shouldExist">Whether every resolved directory must exist.</param>
    /// <param name="rootPaths">The roots to search in order. Defaults to the current working directory.</param>
    /// <returns>The resolved absolute paths in candidate order, or a validation error with no partial results.</returns>
    public static Result<AbsolutePath[], ErrorString> GetDirectoriesResult(string?[]? candidatePaths, bool allowEmpty,
        bool shouldExist, params AbsolutePath[] rootPaths)
    {
        if (candidatePaths is null)
        {
            return new("Directory candidates cannot be null");
        }

        return ResolvePathBatch(candidatePaths.Length == 0 ? [null] : candidatePaths,
            candidate => GetDirectoryResult(candidate, allowEmpty, shouldExist, rootPaths), "directories");
    }

    /// <summary>
    /// Resolves a batch of directory candidates against a predefined root and combines any validation errors.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as one empty candidate.</param>
    /// <param name="allowEmpty">Whether a null, empty, or whitespace candidate may resolve to the root.</param>
    /// <param name="shouldExist">Whether every resolved directory must exist.</param>
    /// <param name="root">The predefined root for relative or allowed empty candidates.</param>
    /// <returns>The resolved absolute paths in candidate order, or a validation error with no partial results.</returns>
    public static Result<AbsolutePath[], ErrorString> GetDirectoriesResult(string?[]? candidatePaths, bool allowEmpty,
        bool shouldExist, PredefinedRootPath root) =>
        GetDirectoriesResult(candidatePaths, allowEmpty, shouldExist, root.GetRootFolder());

    /// <summary>
    /// Resolves a file candidate and validates its existence when requested.
    /// </summary>
    /// <param name="candidatePath">The relative or absolute file candidate. A blank candidate is an error.</param>
    /// <param name="shouldExist">Whether the resolved file must exist.</param>
    /// <param name="rootPaths">The roots to search in order. Defaults to the current working directory.</param>
    /// <returns>The resolved absolute path or a validation error.</returns>
    public static Result<AbsolutePath, ErrorString> GetFileResult(string? candidatePath, bool shouldExist,
        params AbsolutePath[] rootPaths)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            return new("Candidate path cannot be empty");
        }

        return candidatePath.AnalyzeFile(rootPaths).GetPath(shouldExist, originalPathCanBeNull: false);
    }

    /// <summary>
    /// Resolves a file candidate against a predefined root.
    /// </summary>
    /// <param name="candidatePath">The relative or absolute file candidate. A blank candidate is an error.</param>
    /// <param name="shouldExist">Whether the resolved file must exist.</param>
    /// <param name="root">The predefined root for relative candidates.</param>
    /// <returns>The resolved absolute path or a validation error.</returns>
    public static Result<AbsolutePath, ErrorString> GetFileResult(string? candidatePath, bool shouldExist,
        PredefinedRootPath root) =>
        GetFileResult(candidatePath, shouldExist, root.GetRootFolder());

    /// <summary>
    /// Resolves a batch of file candidates and combines any validation errors.
    /// </summary>
    /// <param name="candidatePaths">The file candidates. A null or empty array is an error.</param>
    /// <param name="shouldExist">Whether every resolved file must exist.</param>
    /// <param name="rootPaths">The roots to search in order. Defaults to the current working directory.</param>
    /// <returns>The resolved absolute paths in candidate order, or a validation error with no partial results.</returns>
    public static Result<AbsolutePath[], ErrorString> GetFilesResult(string?[]? candidatePaths, bool shouldExist,
        params AbsolutePath[] rootPaths)
    {
        if (candidatePaths is null || candidatePaths.Length == 0)
        {
            return new ("Paths cannot be empty");
        }

        return ResolvePathBatch(candidatePaths, candidate => GetFileResult(candidate, shouldExist, rootPaths), "files");
    }

    /// <summary>
    /// Resolves a batch of file candidates against a predefined root and combines any validation errors.
    /// </summary>
    /// <param name="candidatePaths">The file candidates. A null or empty array is an error.</param>
    /// <param name="shouldExist">Whether every resolved file must exist.</param>
    /// <param name="root">The predefined root for relative candidates.</param>
    /// <returns>The resolved absolute paths in candidate order, or a validation error with no partial results.</returns>
    public static Result<AbsolutePath[], ErrorString> GetFilesResult(string?[]? candidatePaths, bool shouldExist,
        PredefinedRootPath root) =>
        GetFilesResult(candidatePaths, shouldExist, root.GetRootFolder());

    /// <summary>
    /// Evaluates every candidate in order so batches report all validation failures instead of stopping at the first.
    /// </summary>
    /// <param name="candidatePaths">The non-empty array of candidates to evaluate.</param>
    /// <param name="resolve">The operation that resolves an individual candidate.</param>
    /// <param name="pathKind">The plural description of the candidates used in the combined error.</param>
    /// <returns>All resolved paths, the error for a single candidate, or the combined errors for a batch.</returns>
    private static Result<AbsolutePath[], ErrorString> ResolvePathBatch(string?[] candidatePaths,
        Func<string?, Result<AbsolutePath, ErrorString>> resolve, string pathKind)
    {
        var paths = new AbsolutePath[candidatePaths.Length];
        List<string> errors = [];

        for (var index = 0; index < candidatePaths.Length; index++)
        {
            switch (resolve(candidatePaths[index]))
            {
                case AbsolutePath path:
                    paths[index] = path;
                    break;
                case ErrorString error:
                    errors.Add(error.Error);
                    break;
            }
        }

        if (errors.Count == 0)
        {
            return paths;
        }

        var stringValue = candidatePaths.Length == 1 ? errors[0] : $"{errors.Count} {pathKind}: {string.Join(", ", errors)}";
        return new ErrorString(stringValue);
    }
}
#endif