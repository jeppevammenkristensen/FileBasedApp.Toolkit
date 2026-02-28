using TruePath;

// ReSharper disable InvalidXmlDocComment

namespace FileBasedApp.Toolkit.Abstractions;




public static class AbsolutePathExtensions
{
    internal const int DefaultMaxDepth = 20;
    
    public static AbsolutePath FindParent(this AbsolutePath folder, Func<AbsolutePath, bool> predicate, int maxDepth = DefaultMaxDepth)
    {
        return folder.FindAncestorOrNull(false, predicate, maxDepth) ??
               throw new InvalidOperationException($"Could not find parent folder for path {folder}");
    }
    
    public static AbsolutePath? FindParentOrNull(this AbsolutePath folder, Func<AbsolutePath, bool> predicate, int maxDepth = DefaultMaxDepth)
    {
        return folder.FindAncestorOrNull(false, predicate, maxDepth) ??
               throw new InvalidOperationException($"Could not find parent folder for path {folder}");
    }
    
    internal static AbsolutePath? FindAncestorOrNull(this AbsolutePath folder, bool includeSelf, Func<AbsolutePath, bool> predicate,
        int maxDepth = DefaultMaxDepth)
    {
        return folder
            .GetAncestors(includeSelf, maxDepth)
            .FirstOrDefault(predicate);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static AbsolutePath? AsAbsolutePath(this string? path) => path is null ? null : AbsolutePath.Create(path);
    
    public static AbsolutePath AsRequiredAbsolutePath(this string? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return AbsolutePath.Create(path);
    }
    
    public static AbsolutePath? AsAbsolutePath(this ReadOnlySpan<char> path) => AbsolutePath.Create(path.ToString());

    public static IEnumerable<AbsolutePath> GetAncestors(this AbsolutePath source, bool includeSelf, int maxDepth = DefaultMaxDepth)
    {
        maxDepth = maxDepth < 1 ? DefaultMaxDepth : maxDepth;
        
        if (includeSelf) yield return source;
        int currentDepth = 0;
        var currentPath = source;

        while (currentDepth < maxDepth)
        {
            if (currentPath.Parent is null) yield break;
            currentPath = currentPath.Parent.Value;
            yield return currentPath;
            currentDepth++;
        }
    }
}

