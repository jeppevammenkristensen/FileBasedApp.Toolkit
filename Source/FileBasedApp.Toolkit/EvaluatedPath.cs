using System.ComponentModel;
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
[TypeConverter]
public record EvaluatedPath(string? OriginalPath, AbsolutePath? Path, bool Exists)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="shouldExist"></param>
    /// <param name="originalPathCanBeNull"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public AbsolutePath GetPath(bool shouldExist, bool originalPathCanBeNull)
    {
        if (!originalPathCanBeNull && string.IsNullOrWhiteSpace(OriginalPath))
        {
            throw new InvalidOperationException("OriginalPath cannot be null");
        }

        if (shouldExist && !Exists)
        {
            throw new InvalidOperationException($"Path {OriginalPath} translated to {Path} does not exist");
        }
        
        if (Path is null)
        {
            throw new InvalidOperationException($"Path {OriginalPath} could not be translated to an AbsolutePath");
        }

        

        return Path.Value;

    }
}