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
        /// <remarks>Absolute roots are preserved. Empty paths have no segments; relative parent markers remain individual segments.</remarks>
        public IEnumerable<LocalPath> Traverse()
        {
            var root = path;
            
            // Only when the path is absolute we can use the traversal up
            // it results in an infinite loop when it's a RelativePath
            if (path.IsAbsolute)
            {
                while (root.Parent is { } parent)
                {
                    root = parent;
                }
            }

            var relativeValue = path.IsAbsolute ? path.Value[root.Value.Length..] : path.Value;
            var separator = Path.DirectorySeparatorChar;
            var segments = relativeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (var index = segments.Length - 1; index >= 0; index--)
            {
                yield return new LocalPath(segments[index]);
            }

            if (path.IsAbsolute)
            {
                yield return root;
            }
        }
    }
}