using System.Text.RegularExpressions;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Represents a NuGet package reference with its name and version information parsed from a file-based app directive.
/// </summary>
public partial class PackageInfo
{
    /// <summary>
    /// Represents a NuGet package reference with its name and version information parsed from a file-based app directive.
    /// </summary>
    /// <param name="name">The package name identifier.</param>
    /// <param name="version">The package version string.</param>
    public PackageInfo(string name, string version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>
    /// Gets a compiled regex that matches a package directive in the format <c>packageName@version</c>.
    /// </summary>
    [GeneratedRegex(@"^package (?<packageName>.+)@(?<version>.+)$")]
    public static partial Regex PackageDirectiveRegex { get; }

    /// <summary>The package name identifier.</summary>
    public string Name { get; init; }

    /// <summary>The package version string.</summary>
    public PackageVersion Version { get; init; }

    /// <summary>
    /// Attempts to parse a package directive string in the format <c>packageName@version</c>.
    /// </summary>
    /// <param name="candidate">The directive content to parse.</param>
    /// <returns>A <see cref="PackageInfo"/> if parsing succeeds; otherwise, <see langword="null"/>.</returns>
    public static PackageInfo? SafeParse(string candidate)
    {
        if (PackageDirectiveRegex.Match(candidate) is {Success: true} match)
        {
            return new PackageInfo(match.Groups["packageName"].Value, match.Groups["version"].Value);
        }

        return null;
    }
}