using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides local path traversal and home-directory resolution helpers.
/// </summary>
public static class LocalPathExtensions
{
    extension(LocalPath path)
    {
        /// <summary>
        /// Determines whether the path consists only of the home-directory marker (<c>~</c>).
        /// </summary>
        public bool IsHome => path.Value == "~";

        /// <summary>
        /// Determines whether the normalized path lexically represents the current directory.
        /// </summary>
        /// <remarks>Absolute paths are not treated as current-directory markers, even when they point to the working directory.</remarks>
        public bool IsCurrentDirectory()
        {
            // TruePath 1.x normalizes the current directory to ""; 2.x uses ".".
            return path.Value is "" or ".";
        }

        /// <summary>
        /// Determines whether the path starts at a root directory rather than the current directory.
        /// </summary>
        /// <remarks>
        /// Includes absolute paths such as <c>/logs</c> on Unix and <c>C:\logs</c> on Windows,
        /// as well as Windows paths such as <c>\logs</c> whose drive is unspecified.
        /// Windows drive-relative paths such as <c>C:logs</c> and <c>C:</c> return <see langword="false"/>
        /// because they depend on that drive's current directory. This check does not access the file system.
        /// </remarks>
        public bool HasRoot()
        {
            return path.IsAbsolute || (OperatingSystem.IsWindows() && path.Value.StartsWith('\\'));
        }

        /// <summary>
        /// Determines whether a relative path starts with the home-directory marker (<c>~</c>).
        /// </summary>
        /// <remarks>This will check if the given path is relative and starts with tilde.</remarks>
        public bool HasHomeDirectoryPrefix()
        {
            if (path.IsAbsolute)
            {
                return false;
            }

            var localPaths = path.GetSegments().ToArray();
            if (localPaths is {Length: > 0})
            {
                return localPaths[0].IsHome;
            }

            return false;
        }

        /// <summary>
        /// Replaces a leading home-directory marker with the user profile directory.
        /// </summary>
        /// <returns>An absolute <see cref="LocalPath"/> for a home-based path; otherwise, the original path.</returns>
        public LocalPath ExpandHomeDirectory()
        {
            if (!path.HasHomeDirectoryPrefix())
            {
                return path;
            }

            var root = Environment.SpecialFolder.UserProfile.GetSpecialFolder();

            foreach (var segment in path.GetSegments().Skip(1))
            {
                root /= segment;
            }

            return root;
        }

        /// <summary>
        /// Enumerates the path segments from the root-most segment to the final segment.
        /// </summary>
        public IEnumerable<LocalPath> GetSegments()
        {
            return path.Traverse().Reverse();
        }

        /// <summary>
        /// Enumerates individual path segments from the final segment to the root-most segment.
        /// </summary>
        /// <remarks>
        /// Absolute roots and Windows roots without a drive are preserved as separate segments.
        /// Windows drive-relative prefixes remain attached to the first segment (for example, <c>C:foo</c>).
        /// Empty and current-directory paths have no segments; relative parent markers remain individual segments.
        /// </remarks>
        public IEnumerable<LocalPath> Traverse()
        {
            if (path.IsCurrentDirectory())
            {
                yield break;
            }

            var root = path;
            var hasRoot = path.HasRoot();

            // Rooted paths have a terminating parent chain, even when their drive is unspecified.
            // Relative paths (including C:foo) can grow parent markers indefinitely.
            if (hasRoot)
            {
                while (root.Parent is { } parent)
                {
                    root = parent;
                }
            }

            var relativeValue = hasRoot ? path.Value[root.Value.Length..] : path.Value;
            var separator = Path.DirectorySeparatorChar;
            var segments = relativeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (var index = segments.Length - 1; index >= 0; index--)
            {
                yield return new LocalPath(segments[index]);
            }

            if (hasRoot)
            {
                yield return root;
            }
        }
    }
}