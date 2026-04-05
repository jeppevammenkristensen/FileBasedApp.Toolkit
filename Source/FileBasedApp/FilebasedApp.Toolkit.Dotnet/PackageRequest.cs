namespace FilebasedApp.Toolkit.Dotnet;

/// <summary>
/// Represents the top-level response from <c>dotnet package search --format json</c>.
/// </summary>
public record PackageRequest(
    int Version,
    object[] Problems,
    SearchResult[] SearchResult
);