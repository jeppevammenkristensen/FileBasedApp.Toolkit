using FileBasedApp.Toolkit.SimpleExec;

namespace FileBasedApp.Toolkit.Dotnet;

/// <summary>
/// Higher-level helpers that compose dotnet CLI commands into common workflows.
/// </summary>
public static class DotnetRecipes
{
    /// <summary>
    /// Searches for a NuGet package by name (including prereleases) and returns the parsed result.
    /// </summary>
    public static async Task<PackageRequest> GetPackageInformation(string item, bool includePrerelease)
    {
        return await SimpleExecRunner.Init("dotnet")
            .AddArguments("package", "search")
            .AddArgument(item)
            .AddArgumentPair("--format", "json")
            .AddArgumentConditionally("--prerelease", includePrerelease)
            .ReadAndParseJson<SimpleExecRunner, PackageRequest>(FileBasedApp.Toolkit.Dotnet.AppJsonContext.Default.PackageRequest);
    }
}

/// <summary>
/// Extension members for <see cref="PackageRequest"/> providing convenient querying over search results.
/// </summary>
public static class PackageRequestExtension
{
    extension(PackageRequest request)
    {
        /// <summary>
        /// Returns All packages from all sources flattened into a single enumerable, including the source name for each package.
        /// </summary>
        public IEnumerable<(string SourceName, Package Package)> FlattenedPackages => request.SearchResult.SelectMany(x => x.Packages.Select(y => (x.SourceName, y)));
        
            
        
        /// <summary>
        /// Gets the highest version of a package with the specified name.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Package GetHighestVersion(string item)
        {
            return request.FlattenedPackages
                .Where(x => x.Package.Id.Equals(item, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Package.LatestVersion)
                .Select(x => x.Package)
                .FirstOrDefault() ?? throw new InvalidOperationException($"No package found with id {item}");
        }
    }
}