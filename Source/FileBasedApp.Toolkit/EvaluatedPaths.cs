using System.Collections.Immutable;
using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Represents a collection of paths that have been evaluated for their validity and existence.
/// </summary>
/// <param name="Paths">
/// A list of evaluated paths, where each entry contains details about the original path, its resolved absolute path,
/// and whether it exists on the file system. May include entries with null or invalid data based on evaluation results.
/// </param>
internal record EvaluatedPaths(ImmutableArray<EvaluatedPath> Paths) : IEvaluatedPath
{
    public (AbsolutePath path, string? errorMessage) GetPath(bool shouldExist, bool originalPathCanBeNull)
    {
        if (!shouldExist)
        {
            return Paths[0].GetPath(shouldExist, originalPathCanBeNull);
        }
        else
        {
            var results = Paths.Select(x => x.GetPath(shouldExist, originalPathCanBeNull))
                .Select(x => new {x.errorMessage, x.path})
                .ToList();
            
            var result = results
                .FirstOrDefault(x => x.errorMessage == null) ?? results[0];
            
            return (result.path, result.errorMessage);
        }
    }
}