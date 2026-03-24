using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a path that has been evaluated for its validity and existence.
/// </summary>
/// <param name="OriginalPath">
/// The original path string provided for evaluation. Can be a relative or absolute path.
/// </param>
/// <param name="Path">
/// The resulting absolute path after evaluation. May be null if the path cannot be translated to an absolute path.
/// </param>
/// <param name="Exists">
/// A boolean indicating whether the evaluated path exists on the file system.
/// </param>
internal record EvaluatedPath(string? OriginalPath, AbsolutePath? Path, bool Exists) : IEvaluatedPath
{
    /// <summary>
    /// Return a tuple with the Path and string representing an error message if the path is not valid or does not exist. The parameters allow for flexible validation based on the context in which the path is being used.
    /// </summary>
    /// <param name="shouldExist"></param>
    /// <param name="originalPathCanBeNull"></param>
    /// <returns></returns>
    public (AbsolutePath path, string? errorMessage) GetPath(bool shouldExist, bool originalPathCanBeNull)
    {
        if (!originalPathCanBeNull && string.IsNullOrWhiteSpace(OriginalPath))
        {
            return (default, "OriginalPath cannot be null");
        }

        if (shouldExist && !Exists)
        {
            return (default, $"Path {OriginalPath} translated to {Path} does not exist");
        }
        
        if (Path is null)
        {
            return (default,$"Path {OriginalPath} could not be translated to an AbsolutePath");
        }

        return (Path.Value, null);

    }
}