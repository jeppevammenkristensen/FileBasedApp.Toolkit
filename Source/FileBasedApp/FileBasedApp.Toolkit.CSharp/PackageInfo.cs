using System.Text.RegularExpressions;

namespace FileBasedApp.Toolkit.CSharp;


public enum PackageVersionType
{
    /// <summary>
    /// A flexible semantic version (up to 4 numbers), e.g. "1.2.3", "1.2.3-alpha", "1.2.3.4-alpha-01
    /// </summary>
    SemVer,
    /// <summary>
    /// A star indicating any version is acceptable, e.g. "*"
    /// </summary>
    Star, 
    /// <summary>
    /// Unknown version type
    /// </summary>
    Unknown
}

public partial class PackageVersion 
{
    /// <summary>
    /// The type of the package version, e.g. semantic version, star, or unknown.
    /// </summary>
    public PackageVersionType Type { get; private set; } = PackageVersionType.Unknown;
    
    public string Value
    {
        get => field;
        set
        {
            field = value;
            EvaluteAndSet(value);
        }
    }
    
    public bool IsCatchAll => Value == "*";

    private Match? _semVarMatch;

    public string? VersionSuffix => _semVarMatch?.Groups["versionsuffix"].Value;
    public string? VersionPrefix => _semVarMatch?.Groups["versionprefix"].Value;
    
    public string? Major => _semVarMatch?.Groups["major"].Value;
    public string? Minor => _semVarMatch?.Groups["minor"].Value;
    public string? Patch => _semVarMatch?.Groups["patch"].Value;
    public string? Revision => _semVarMatch?.Groups["revision"].Value;

    public PackageVersion(string version)
    {
        Value = version;
    }

    [GeneratedRegex(@"^(?<versionprefix>(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(\.(?<revision>\d+))?)(-(?<versionsuffix>(?<prerelease>[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*)))?$")]
    private partial Regex VersionRegex { get;  }

    private void EvaluteAndSet(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (VersionRegex.Match(value) is {Success: true} match)
        {
            _semVarMatch = match;
            Type = PackageVersionType.SemVer;
        }
        else if (value == "*")
        {
            _semVarMatch = null;
            Type = PackageVersionType.Star;
        }
        else
        {
            _semVarMatch = null;
            Type = PackageVersionType.Unknown;
        }
        
        
    }
}

/// <summary>
/// Represents a NuGet package reference with its name and version information parsed from a file-based app directive.
/// </summary>
/// <param name="Name">The package name identifier.</param>
/// <param name="Version">The package version string.</param>
public partial record PackageInfo(string Name, string Version)
{
    /// <summary>
    /// Gets a compiled regex that matches a package directive in the format <c>packageName@version</c>.
    /// </summary>
    [GeneratedRegex(@"^package (?<packageName>.+)@(?<version>.+)$")]
    public static partial Regex PackageDirectiveRegex { get; }

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