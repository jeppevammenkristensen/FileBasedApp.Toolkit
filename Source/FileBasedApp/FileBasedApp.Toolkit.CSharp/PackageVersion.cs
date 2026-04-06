using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace FileBasedApp.Toolkit.CSharp;

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
        get;
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

    //private Match? _semVarMatch;
    private NuGetVersion? _nugetVersion;

    /// <summary>
    /// Gets the pre-release suffix (e.g. <c>beta</c>, <c>alpha.1</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionSuffix => _nugetVersion?.Release.NullIfEmpty();

    /// <summary>
    /// Gets the numeric version prefix (e.g. <c>1.2.3</c> or <c>1.2.3.4</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionPrefix => _nugetVersion?.OriginalVersion?.Split('-')[0]; //_semVarMatch?.Groups["versionprefix"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the major version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Major => _nugetVersion?.Major.ToString(); //   _semVarMatch?.Groups["major"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the minor version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Minor => _nugetVersion?.Minor.ToString(); //?.Groups["minor"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the patch version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Patch => _nugetVersion?.Patch.ToString(); // _semVarMatch?.Groups["patch"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the optional fourth version component (revision), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Revision => _nugetVersion?.Revision.ToString(); //.Groups["revision"].Value.NullIfEmpty();

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
        if (NuGetVersion.TryParse(value, out var nugetVersion))
        {
            _nugetVersion = nugetVersion;
            Type = PackageVersionType.SemVer;
        }
        else if (value == "*")
        {
            _nugetVersion = null;
            Type = PackageVersionType.Star;
        }
        else
        {
            _nugetVersion = null;
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