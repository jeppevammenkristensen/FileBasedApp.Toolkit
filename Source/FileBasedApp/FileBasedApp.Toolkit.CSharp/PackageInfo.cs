using System.Text.RegularExpressions;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Specifies the type of package version format used.
/// </summary>
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

/// <summary>
/// Represents a parsed package version string, supporting semantic versioning (up to 4-part) with optional pre-release suffixes, wildcard (*), or unknown formats.
/// </summary>
public partial class PackageVersion
{
    /// <summary>
    /// The type of the package version, e.g. semantic version, star, or unknown.
    /// </summary>
    public PackageVersionType Type { get; private set; } = PackageVersionType.Unknown;
    
    /// <summary>
    /// Gets or sets the raw version string. Setting this value re-evaluates the parsed components and <see cref="Type"/>.
    /// </summary>
    public string Value
    {
        get => field;
        set
        {
            field = value;
            EvaluteAndSet(value);
        }
    }
    
    /// <summary>
    /// Gets a value indicating whether this version is a wildcard (<c>*</c>), accepting any version.
    /// </summary>
    public bool IsCatchAll => Value == "*";

    private Match? _semVarMatch;

    /// <summary>
    /// Gets the pre-release suffix (e.g. <c>beta</c>, <c>alpha.1</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionSuffix => _semVarMatch?.Groups["versionsuffix"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the numeric version prefix (e.g. <c>1.2.3</c> or <c>1.2.3.4</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionPrefix => _semVarMatch?.Groups["versionprefix"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the major version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Major => _semVarMatch?.Groups["major"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the minor version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Minor => _semVarMatch?.Groups["minor"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the patch version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Patch => _semVarMatch?.Groups["patch"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the optional fourth version component (revision), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Revision => _semVarMatch?.Groups["revision"].Value.NullIfEmpty();

    /// <summary>
    /// Initializes a new instance of <see cref="PackageVersion"/> by parsing the specified version string.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
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
    
    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj switch
    {
        PackageVersion other => Value == other.Value,
        string other => Value == other,
        _ => false
    };

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Defines an implicit conversion from a <see cref="string"/> to a <see cref="PackageVersion"/>. The string is parsed to determine the version type and components.
    /// </summary>
    /// <param name="version">The version string to convert.</param>
    /// <returns>A new <see cref="PackageVersion"/> parsed from the string.</returns>
    public static implicit operator PackageVersion(string version) => new PackageVersion(version);

    /// <summary>
    /// Defines an implicit conversion from a <see cref="PackageVersion"/> to a <see cref="string"/>, returning the raw version value.
    /// </summary>
    /// <param name="version">The <see cref="PackageVersion"/> to convert.</param>
    /// <returns>The raw version string.</returns>
    public static implicit operator string(PackageVersion version) => version.Value;
}

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