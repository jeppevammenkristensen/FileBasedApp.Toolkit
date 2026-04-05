namespace FileBasedApp.Toolkit.Dotnet;

/// <summary>
/// Represents package metadata returned by the package search response.
/// </summary>
public record Package(
    string Id,
    string LatestVersion,
    int TotalDownloads,
    string Owners
);