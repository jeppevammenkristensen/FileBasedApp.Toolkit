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