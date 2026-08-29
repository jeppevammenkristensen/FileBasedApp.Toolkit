namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Retrieves the first element matching the predicate.
        /// </summary>
        /// <param name="predicate">An optional function to test each element.</param>
        /// <param name="noMatchErrorMessage">An optional message used when no element matches.</param>
        /// <returns>The first matching element.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no element matches.</exception>
        public T GetFirstRequired(Func<T, bool>? predicate = null, string? noMatchErrorMessage = null)
        {
            noMatchErrorMessage ??= "No item found";
            return GetRequired(source, predicate, candidates => candidates.Count switch
            {
                0 => throw new InvalidOperationException(noMatchErrorMessage),
                _ => candidates[0]
            });
        }
    }

    extension<T>(IEnumerable<T> source) where T : struct
    {
        /// <summary>
        /// Retrieves the first struct element matching the predicate, or null if none are found.
        /// </summary>
        /// <param name="predicate">An optional function to test each element.</param>
        /// <returns>The first matching element, or null.</returns>
        public T? GetFirstOrNull(Func<T, bool>? predicate = null)
        {
            return GetOptional(source, predicate, candidates => candidates.Count switch
            {
                0 => null,
                _ => candidates[0]
            });
        }

        /// <summary>
        /// Retrieves the single struct element matching the predicate.
        /// </summary>
        /// <param name="predicate">An optional function to test each element.</param>
        /// <param name="noMatchErrorMessage">An optional message used when no element matches.</param>
        /// <param name="multipleMatchesError">An optional message used when multiple elements match.</param>
        /// <returns>The single matching element.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no elements or multiple elements match.</exception>
        public T GetSingleRequired(Func<T, bool>? predicate = null,
            string? noMatchErrorMessage = null, string? multipleMatchesError = null)
        {
            noMatchErrorMessage ??= "No item found";
            multipleMatchesError ??= "Found more than 1 matches";

            return GetRequired(source, predicate, candidates => candidates switch
            {
                { Count: 0 } => throw new InvalidOperationException(noMatchErrorMessage),
                { Count: 1 } => candidates[0],
                _ => throw new InvalidOperationException(multipleMatchesError)
            });
        }

        /// <summary>
        /// Retrieves the single struct element matching the predicate, or null if none are found.
        /// </summary>
        /// <param name="predicate">An optional function to test each element.</param>
        /// <param name="multipleMatchesError">An optional message used when multiple elements match.</param>
        /// <returns>The single matching element, or null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when multiple elements match.</exception>
        public T? GetSingleOrNull(Func<T, bool>? predicate = null, string? multipleMatchesError = null)
        {
            multipleMatchesError ??= "Found more than 1 matches";

            return GetOptional(source, predicate, candidates => candidates switch
            {
                { Count: 0 } => null,
                { Count: 1 } => candidates[0],
                _ => throw new InvalidOperationException(multipleMatchesError)
            });
        }
    }

    extension<T>(IEnumerable<T?> source) where T : struct
    {
        /// <summary>
        /// Retrieves the single nullable struct element matching the predicate, or null if none are found.
        /// </summary>
        /// <param name="predicate">An optional function to test each element.</param>
        /// <param name="multipleMatchesError">An optional message used when multiple elements match.</param>
        /// <returns>The single matching element, or null.</returns>
        /// <exception cref="InvalidOperationException">Thrown when multiple elements match.</exception>
        public T? GetSingleOrNull(Func<T?, bool>? predicate = null, string? multipleMatchesError = null)
        {
            multipleMatchesError ??= "Found more than 1 matches";

            return GetOptional(source, predicate, candidates => candidates switch
            {
                { Count: 0 } => null,
                { Count: 1 } => candidates[0],
                _ => throw new InvalidOperationException(multipleMatchesError)
            });
        }
    }

    private static T GetRequired<T>(IEnumerable<T> source, Func<T, bool>? predicate,
        Func<IReadOnlyList<T>, T> resultGenerator)
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }

    private static T? GetOptional<T>(IEnumerable<T> source, Func<T, bool>? predicate,
        Func<IReadOnlyList<T>, T?> resultGenerator) where T : struct
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }

    private static T? GetOptional<T>(IEnumerable<T?> source, Func<T?, bool>? predicate,
        Func<IReadOnlyList<T?>, T?> resultGenerator) where T : struct
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }
}
