using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Represents a parsed package version string, supporting semantic versioning (up to 4-part) with optional pre-release suffixes, wildcard (*), or unknown formats.
/// </summary>
/// <remarks>If a semantic version is present it can be be accessed through the <see cref="NugetVersion"/> or <see cref="RequiredNugetVersion"/></remarks>
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
        private set;
    }
    
    /// <summary>
    /// Gets a value indicating whether this version is a wildcard (<c>*</c>), accepting any version.
    /// </summary>
    public bool IsCatchAll => Value == "*";

    
    /// <summary>
    /// The parsed NuGet version, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    /// <remarks>When the setter is called with a non null instance the Type is changed to SemVer</remarks>
    public NuGetVersion? NugetVersion
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the NuGet semantic version, throwing an exception if the version is not a valid semantic version.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the version is not a semantic version (e.g., wildcard or unknown format).</exception>
    public NuGetVersion RequiredNugetVersion => NugetVersion ?? throw new InvalidOperationException("This version is not a semantic version");

    /// <summary>
    /// Gets the pre-release suffix (e.g. <c>beta</c>, <c>alpha.1</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionSuffix => NugetVersion?.Release.NullIfEmpty();

    /// <summary>
    /// Gets the numeric version prefix (e.g. <c>1.2.3</c> or <c>1.2.3.4</c>), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? VersionPrefix => NugetVersion?.OriginalVersion?.Split('-')[0]; //_semVarMatch?.Groups["versionprefix"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the major version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Major => NugetVersion?.Major.ToString(); //   _semVarMatch?.Groups["major"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the minor version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Minor => NugetVersion?.Minor.ToString(); //?.Groups["minor"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the patch version component, or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Patch => NugetVersion?.Patch.ToString(); // _semVarMatch?.Groups["patch"].Value.NullIfEmpty();

    /// <summary>
    /// Gets the optional fourth version component (revision), or <see langword="null"/> if the version is not a semantic version.
    /// </summary>
    public string? Revision => NugetVersion?.Revision.ToString(); //.Groups["revision"].Value.NullIfEmpty();

    /// <summary>
    /// Initializes a new instance of <see cref="PackageVersion"/> by parsing the specified version string.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    public PackageVersion(string version)
    {
        Value = version;
        EvaluteAndSet(version);
    }

    /// <summary>
    /// Replaces the NuGet version with a new instance and returns the current instance
    /// </summary>
    /// <param name="newVersion"></param>
    /// <returns></returns>
    public PackageVersion WithNugetVersion(NuGetVersion newVersion)
    {
        NugetVersion = newVersion;
        Type = PackageVersionType.SemVer;
        Value = newVersion.ToString();

        return this;
    }

    /// <summary>
    /// Applies a transformation function to the current NuGet version and updates the package version accordingly, returning the current instance.
    /// </summary>
    /// <param name="versionModifier">A function that takes the current NuGet version and returns a modified NuGet version.</param>
    /// <return>The current <see cref="PackageVersion"/> instance with the updated NuGet version.</return>
    /// <remarks>Use this in combination with the extension methods for NugetVersion. For instance WithMajor.</remarks>
    public PackageVersion WithNugetVersion(Func<NuGetVersion, NuGetVersion> versionModifier) => WithNugetVersion(versionModifier(RequiredNugetVersion));

    /// <summary>
    /// Adds a new version string and re-evaluates the version components
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public PackageVersion WithValue(string version)
    {
        Value = version;
        EvaluteAndSet(version);
        return this;
    }

    [GeneratedRegex(@"^(?<versionprefix>(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(\.(?<revision>\d+))?)(-(?<versionsuffix>(?<prerelease>[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*)))?$")]
    private partial Regex VersionRegex { get;  }

    private void EvaluteAndSet(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (NuGetVersion.TryParse(value, out var nugetVersion))
        {
            // Type is set to SemVer when a nuget version is not null
            WithNugetVersion(nugetVersion);
        }
        else if (value == "*")
        {
            NugetVersion = null;
            Type = PackageVersionType.Star;
        }
        else
        {
            NugetVersion = null;
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