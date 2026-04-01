using System.Text.RegularExpressions;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Regexes related to CSharp/Roslyn
/// </summary>
public static partial class CSharpRegex
{
    /// <summary>
    /// Matches file-based app directives in leading trivia, e.g. <c>#:package</c>, <c>#:property</c>, <c>#:sdk</c>, <c>#:project</c>.
    /// </summary>


#if NET10_0
    [GeneratedRegex("^(package|property|sdk|project)")]
        public static partial Regex HasFileBasedDirectiveRegex { get; }
#else
    public static Regex HasFileBasedDirectiveRegex => _hasFileBasedDirectiveRegex();

    [GeneratedRegex("^#:(package|property|sdk|project)")]
    private static partial Regex _hasFileBasedDirectiveRegex();
#endif
}