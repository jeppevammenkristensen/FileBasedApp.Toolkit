using System.Diagnostics;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extensions for the PathUtil class
/// </summary>
public static class PathUtilExtensions
{
    /// <inheritdoc cref="PathUtil.GetRootFolder(PredefinedRootPath)"/>
    public static AbsolutePath GetRootFolder(this PredefinedRootPath path) =>PathUtil.GetRootFolder(path);
    
    /// <inheritdoc cref="PathUtil.AnalyzeFile(string, AbsolutePath[])"/>
    public static IEvaluatedPath AnalyzeFile(this string? pathCandidate, params AbsolutePath[] rootPaths) => PathUtil.AnalyzeFile(pathCandidate, rootPaths);

    /// <inheritdoc cref="PathUtil.AnalyzeDirectory(string, AbsolutePath[])"/>
    public static IEvaluatedPath AnalyzeDirectory(this string? pathCandidate, params AbsolutePath[] rootPaths) =>
        PathUtil.AnalyzeDirectory(pathCandidate, rootPaths);
}