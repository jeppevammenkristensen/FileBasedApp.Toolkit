using System.IO.Abstractions;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Provides extension methods for evaluating files in the context of file-based applications,
/// enabling convenient detection and analysis of file-based app directives and characteristics.
/// </summary>
public static class FileEvaluationExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the file at <paramref name="path"/> is a file-based app,
    /// determined by the presence of a file-based directive in the leading trivia of a top-level script.
    /// </summary>
    /// <param name="path">The path to the file to evaluate.</param>
    /// <param name="requireGlobalStatement">Require that the first statement is a Global statement (lines of code) <br/>
    /// <![CDATA[Console.WriteLine("Hello")]]></param>
    /// <param name="fileSystem">Optional file system abstraction for file operations. If not provided, uses the default file system.</param>
    /// <returns><see langword="true"/> if the file is a file-based app; otherwise, <see langword="false"/>.</returns>
    public static bool IsFileBasedApp(this AbsolutePath path, bool requireGlobalStatement = true,
        IFileSystem? fileSystem = null)
    {
        return new FileBasedAppEvaluator(fileSystem ?? new FileSystem()).IsFileBasedApp(path, requireGlobalStatement);
    }

    /// <summary>
    /// Asynchronously returns <see langword="true"/> if the file at <paramref name="pth"/> is a file-based app,
    /// determined by the presence of a file-based directive in the leading trivia of a top-level script.
    /// </summary>
    /// <param name="pth">The path to the file to evaluate.</param>
    /// <param name="requireGlobalStatement">Require that the first statement is a Global statement (lines of code) <br/>
    /// <![CDATA[Console.WriteLine("Hello")]]></param>
    /// <param name="fileSystem">Optional file system abstraction for file operations. If not provided, uses the default file system.</param>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the file is a file-based app; otherwise, <see langword="false"/>.</returns>
    public static Task<bool> IsFileBasedAppAsync(this AbsolutePath pth, bool requireGlobalStatement = true,
        IFileSystem? fileSystem = null, CancellationToken token = default)
    {
        return new FileBasedAppEvaluator(fileSystem ?? new FileSystem()).IsFileBasedAppAsync(pth, requireGlobalStatement,token);
    }
}