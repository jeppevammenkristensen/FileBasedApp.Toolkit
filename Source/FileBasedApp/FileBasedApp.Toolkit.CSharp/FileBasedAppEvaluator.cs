using System.IO.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;



/// <summary>
/// Evaluates if a file is a file-based app by inspecting its leading Roslyn trivia
/// for file-based directives such as <c>#:package</c>, <c>#:property</c>, <c>#:sdk</c>, or <c>#:project</c>.
/// </summary>
public class FileBasedAppEvaluator
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Evaluates if a file is a file-based app by inspecting its leading Roslyn trivia
    /// for file-based directives such as <c>#:package</c>, <c>#:property</c>, <c>#:sdk</c>, or <c>#:project</c>.
    /// </summary>
    public FileBasedAppEvaluator() : this(new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBasedAppEvaluator"/> class with a custom file system implementation.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use for file operations.</param>
    public FileBasedAppEvaluator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
 

    /// <summary>
    /// Returns <see langword="true"/> if the file at <paramref name="path"/> is a file-based app,
    /// determined by the presence of a file-based directive in the leading trivia of a top-level script.
    /// </summary>
    /// <param name="path">The path to the file to evaluate.</param>
    /// <param name="requireGlobalStatement">Require that the first statement is a Global statement (lines of code) <br/>
    /// <![CDATA[Console.WriteLine("Hello")]]></param>
    public bool IsFileBasedApp(AbsolutePath path, bool requireGlobalStatement = true)
    {
        var compilationUnit = SyntaxFactory.ParseCompilationUnit(_fileSystem.File.ReadAllText(path.Value));

        // Only top-level (script-style) files qualify
        if (requireGlobalStatement && compilationUnit.Members.FirstOrDefault() is not GlobalStatementSyntax)
            return false;

        if (compilationUnit.GetLeadingTrivia().Count == 0)
        {
            return compilationUnit.Members.FirstOrDefault() is GlobalStatementSyntax;
        }
        
        return compilationUnit.GetLeadingTrivia()
            .Any(IsSupportedFileBaseTrivia);
    }

    private bool IsSupportedFileBaseTrivia(SyntaxTrivia trivia)
    {
        if (!trivia.IsKind(SyntaxKind.IgnoredDirectiveTrivia))
        {
            return false;
        }

        if (trivia.GetStructure() is IgnoredDirectiveTriviaSyntax { } ignored)
        {
            return CSharpRegex.HasFileBasedDirectiveRegex.IsMatch(ignored.Content.Text);
        }

        return false;
    }

    /// <summary>
    /// Asynchronously returns <see langword="true"/> if the file at <paramref name="path"/> is a file-based app,
    /// determined by the presence of a file-based directive in the leading trivia of a top-level script.
    /// </summary>
    /// <param name="path">The path to the file to evaluate.</param>
    /// <param name="requireGlobalStatement">Require that the first statement is a Global statement (lines of code) <br/>
    /// <![CDATA[Console.WriteLine("Hello")]]></param>
    /// <param name="token">The cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the file is a file-based app; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> IsFileBasedAppAsync(AbsolutePath path, bool requireGlobalStatement = true,
        CancellationToken token = default)
    {
        var compilationUnit = SyntaxFactory.ParseCompilationUnit(await _fileSystem.File.ReadAllTextAsync(path.Value, token));

        // Only top-level (script-style) files qualify
        if (requireGlobalStatement && compilationUnit.Members.FirstOrDefault() is not GlobalStatementSyntax)
            return false;

        return compilationUnit.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.IgnoredDirectiveTrivia));
    }
}
