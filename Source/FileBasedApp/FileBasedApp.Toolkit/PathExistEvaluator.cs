using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a delegate used to determine whether a given absolute path exists.
/// </summary>
/// <param name="path">The absolute path to evaluate.</param>
/// <returns>A boolean value indicating whether the specified path exists.</returns>
internal delegate bool PathExistEvaluator(AbsolutePath path);