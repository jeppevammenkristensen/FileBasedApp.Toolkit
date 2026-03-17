using System.Text.RegularExpressions;

namespace FileBasedApp.Toolkit.CSharp;

public static partial class CsharpRegex
{
    /// <summary>
    /// Matches file-based app directives in leading trivia, e.g. <c>#:package</c>, <c>#:property</c>, <c>#:sdk</c>, <c>#:project</c>.
    /// </summary>
    [GeneratedRegex("^#:(package|property|sdk|project)")]
    public static partial Regex HasFileBasedDirectiveRegex { get; }
}