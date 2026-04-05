namespace FilebasedApp.Toolkit.Dotnet;

/// <summary>
/// Represents a search source and the packages returned from it.
/// </summary>
public record SearchResult(
    string SourceName,
    Package[] Packages
);