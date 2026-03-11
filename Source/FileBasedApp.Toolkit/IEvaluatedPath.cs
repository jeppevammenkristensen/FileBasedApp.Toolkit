using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// This contains information based on the evaluation of a path.
/// </summary>
public interface IEvaluatedPath
{
    /// <summary>
    /// Evalutes and retur the given path or an error message
    /// </summary>
    /// <param name="shouldExist"></param>
    /// <param name="originalPathCanBeNull"></param>
    /// <returns>Returns a tuple containing the Path and an errormessage. If the error message is null the path is invalid</returns>
    (AbsolutePath path, string? errorMessage) GetPath(bool shouldExist, bool originalPathCanBeNull);
}