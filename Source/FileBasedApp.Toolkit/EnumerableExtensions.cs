namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extension method for <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Retrieves the first struct element from the provided collection that matches the predicate, if none are found
    /// an exception is thrown
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <param name="noMatchErrorMessage"></param>
    /// <typeparam name="TStruct"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TStruct GetFirstRequired<TStruct>(this IEnumerable<TStruct> source,
        Func<TStruct, bool>? predicate = null,
        string? noMatchErrorMessage = null)
    {
        noMatchErrorMessage ??= "No item found";
        return GetRequiredStruct(source, predicate, candidates => candidates.Count switch
        {
            0 => throw new InvalidOperationException(noMatchErrorMessage),
            _ => candidates[0]
        });
    }

    /// <summary>
    /// Retrieves the first struct element from the provided collection that matches the predicate, if none are found
    /// null is returned
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="TStruct"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TStruct? GetFirstOrNull<TStruct>(this IEnumerable<TStruct> source,
        Func<TStruct, bool>? predicate = null ) where TStruct : struct
    {
        
        return GetOptionalStruct(source, predicate, candidates => candidates.Count switch
        {
            0 => null,
            _ => candidates[0]
        });
    }
    
    /// <summary>
    /// Retrieves the single struct element from the provided collection that matches the predicate,
    /// throwing an exception if zero or multiple elements are found. 
    /// </summary>
    /// <param name="source">The collection of elements to search through.</param>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <param name="noMatchErrorMessage">An optional custom error message to display when no matching element is found.</param>
    /// <param name="multipleMatchesError">An optional custom error message to display when multiple matching elements are found.</param>
    /// <typeparam name="TStruct">The type of the elements in the collection, restricted to value types.</typeparam>
    /// <returns>The single element from the collection that matches the specified condition.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no elements or multiple elements match the specified condition.</exception>
    public static TStruct GetSingleRequired<TStruct>(this IEnumerable<TStruct> source, Func<TStruct, bool>? predicate = null,
        string? noMatchErrorMessage = null, string? multipleMatchesError = null) where TStruct : struct
    {
        noMatchErrorMessage ??= "No item found";
        multipleMatchesError ??= "Found more than 1 matches";

        return GetRequiredStruct(source, predicate, candidates => candidates switch
        {
            {Count: 0} => throw new InvalidOperationException(noMatchErrorMessage),
            {Count: 1} => candidates[0], 
            _ => throw new InvalidOperationException(multipleMatchesError)
        });
    }
    
    /// <summary>
    /// Retrieves the single struct element from the provided collection that matches the predicate or null if none are found
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate">An optional prediate for filtering</param>
    /// <param name="multipleMatchesError">The error text to present if there are multiple matches</param>
    /// <typeparam name="TStruct"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>This is here to aid when dealing with structs</remarks>
    public static TStruct? GetSingleOrNull<TStruct>(this IEnumerable<TStruct> source, Func<TStruct, bool>? predicate = null,
        string? multipleMatchesError = null) where TStruct : struct
    {
        multipleMatchesError ??= "Found more than 1 matches";

        return GetOptionalStruct(source, predicate, candidates => candidates switch
        {
            {Count: 0} => null,
            {Count: 1} => candidates[0], 
            _ => throw new InvalidOperationException(multipleMatchesError)
        });
    }
    
    /// <summary>
    /// Retrieves the single struct element from the provided collection that matches the predicate or null if none are found
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate">An optional prediate for filtering</param>
    /// <param name="multipleMatchesError">The error text to present if there are multiple matches</param>
    /// <typeparam name="TStruct"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>This is here to aid when dealing with structs</remarks>
    public static TStruct? GetSingleOrNull<TStruct>(this IEnumerable<TStruct?> source, Func<TStruct?, bool>? predicate = null,
        string? multipleMatchesError = null) where TStruct : struct
    {
        multipleMatchesError ??= "Found more than 1 matches";

        return GetOptionalStruct(source, predicate, candidates => candidates switch
        {
            {Count: 0} => null,
            {Count: 1} => candidates[0], 
            _ => throw new InvalidOperationException(multipleMatchesError)
        });
    }

    /// <summary>
    /// Retrieves a single, required item from a collection based on the specified predicate or generates a result if multiple matches are found.
    /// </summary>
    /// <param name="source">The collection of items to search through.</param>
    /// <param name="predicate">An optional predicate to filter items in the collection. If null, all items are considered.</param>
    /// <param name="resultGenerator">A function to determine the resulting item when multiple matches are found in the collection.</param>
    /// <typeparam name="TStruct">The type of the items in the collection.</typeparam>
    /// <returns>The single item that matches the predicate or is determined by the result generator.</returns>
    private static TStruct GetRequiredStruct<TStruct>(this IEnumerable<TStruct> source, Func<TStruct, bool>? predicate,
        Func<IReadOnlyList<TStruct>, TStruct> resultGenerator)
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }


    private static TStruct? GetOptionalStruct<TStruct>(this IEnumerable<TStruct> source, Func<TStruct, bool>? predicate,
        Func<IReadOnlyList<TStruct>, TStruct?> resultGenerator) where TStruct : struct
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }
    
    private static TStruct? GetOptionalStruct<TStruct>(this IEnumerable<TStruct?> source, Func<TStruct?, bool>? predicate,
        Func<IReadOnlyList<TStruct?>, TStruct?> resultGenerator) where TStruct : struct
    {
        predicate ??= _ => true;
        var candidates = source.Where(predicate).ToList();
        return resultGenerator(candidates);
    }
}