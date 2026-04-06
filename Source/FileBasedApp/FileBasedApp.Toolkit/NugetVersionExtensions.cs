using NuGet.Versioning;

namespace FileBasedApp.Toolkit;

public static class NugetVersionExtensions
{
    extension(NuGetVersion version)
    {
        /// <summary>
        /// Creates a new NuGetVersion with the specified major version. 
        /// </summary>
        /// <param name="major"></param>
        /// <returns>version 1.0.0 called with 2 would produce 2.0.0</returns>
        public NuGetVersion WithMajor(int major) => new NuGetVersion(major, version.Minor, version.Patch, version.Revision, version.ReleaseLabels, version.Metadata);
        /// <summary>
        /// Creates a new NuGetVersion with the specified minor version.
        /// </summary>
        /// <param name="minor"></param>
        /// <returns>version 1.2.0 called with 3 would produce 1.3.0</returns>
        public NuGetVersion WithMinor(int minor) => new NuGetVersion(version.Major, minor, version.Patch, version.Revision, version.ReleaseLabels, version.Metadata);
        /// <summary>
        /// Creates a new NuGetVersion with the specified patch version.
        /// </summary>
        /// <param name="patch"></param>
        /// <returns>version 1.2.3 called with 4 would produce 1.2.4</returns>
        public NuGetVersion WithPatch(int patch) => new NuGetVersion(version.Major, version.Minor, patch, version.Revision, version.ReleaseLabels, version.Metadata);
        /// <summary>
        /// Creates a new NuGetVersion with the specified revision.
        /// </summary>
        /// <param name="revision"></param>
        /// <returns>version 1.2.3.4 called with 5 would produce 1.2.3.5</returns>
        public NuGetVersion WithRevision(int revision) => new NuGetVersion(version.Major, version.Minor, version.Patch, revision, version.ReleaseLabels, version.Metadata);

        /// <summary>
        /// Creates a new NuGetVersion with the specified release label.
        /// </summary>
        /// <param name="release"></param>
        /// <returns>version 1.2.3 called with "beta" would produce 1.2.3-beta</returns>
        public NuGetVersion WithRelease(string release) => new NuGetVersion(version.Major, version.Minor, version.Patch, version.Revision, [release], version.Metadata);
        
        
    }
}