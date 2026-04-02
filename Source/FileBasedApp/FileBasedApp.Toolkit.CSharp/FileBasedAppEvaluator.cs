using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

public enum FileBasedDirectiveType
{
    Package,
    Property,
    Sdk,
    Project,
    Unknown
}

/// <summary>
/// Evaluates if a file is a file-based app by inspecting its leading Roslyn trivia
/// for file-based directives such as <c>#:package</c>, <c>#:property</c>, <c>#:sdk</c>, or <c>#:project</c>. or shebang 
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
        return IsFileBasedApp(_fileSystem.File.ReadAllText(path.Value));
    }
    
    private static readonly HashSet<SyntaxKind> FileBasedTrivia = [SyntaxKind.IgnoredDirectiveTrivia, SyntaxKind.ShebangDirectiveTrivia];

    private bool IsSupportedFileBaseTrivia(SyntaxTrivia trivia)
    {
        if (!FileBasedTrivia.Contains(trivia.Kind()))
        {
            return false;
        }
        
        if (trivia.IsKind(SyntaxKind.ShebangDirectiveTrivia))
        {
            return true;
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
        var allText = await _fileSystem.File.ReadAllTextAsync(path.Value, token);
        return IsFileBasedApp(allText, requireGlobalStatement);
    }

    /// <summary>
    /// Asynchronously returns <see langword="true"/> if the <see cref="csharpText"/> is a file-based app,
    /// determined by the presence of a file-based directive in the leading trivia of a top-level script.
    /// </summary>
    /// <param name="csharpText"></param>
    /// <param name="requireGlobalStatement">Require that the first statement is a Global statement (lines of code) <br/>
    /// <![CDATA[Console.WriteLine("Hello")]]></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the file is a file-based app; otherwise, <see langword="false"/>.</returns>
    public bool IsFileBasedApp(string csharpText, bool requireGlobalStatement = true)
    {
        var compilationUnit = SyntaxFactory.ParseCompilationUnit(csharpText);

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
}

public class FileBasedAppWrapper
{
    public AbsolutePath Path { get; private set; }
    public CompilationUnitSyntax CompilationUnitSyntax { get; set; }

    public FileBasedAppWrapper(AbsolutePath path, IFileSystem? fileSystem = null)
    {
        var isFileBasedApp = path.IsFileBasedApp(true, fileSystem);
        if (!isFileBasedApp)
        {
            throw new ArgumentException("The file is not a file-based app");
        }

        Path = path;
        CompilationUnitSyntax = SyntaxFactory.ParseCompilationUnit(path.ReadAllText(fileSystem));
    }
    
    private IEnumerable<IgnoredDirectiveWrapper> GetFileBasedDirectives()
    {
        return CompilationUnitSyntax.GetLeadingTrivia()
            .SelectMany(IgnoredDirectiveWrapper.FromSyntaxTriva);
    }
}

internal record IgnoredDirectiveWrapper(FileBasedDirectiveType Directive, string Content, SyntaxTrivia trivia)
{
    public static IEnumerable<IgnoredDirectiveWrapper> FromSyntaxTriva(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.IgnoredDirectiveTrivia))
        {
            if (trivia.GetStructure() is IgnoredDirectiveTriviaSyntax ignoredDirectiveTriviaSyntax)
            {
                if (CSharpRegex.HasFileBasedDirectiveRegex.Match(ignoredDirectiveTriviaSyntax.Content.Text) is
                    {Success: true} match)
                {
                    FileBasedDirectiveType directive = match.Groups["type"].Value switch
                    {
                        "package" => FileBasedDirectiveType.Package,
                        "property" => FileBasedDirectiveType.Property,
                        "sdk" => FileBasedDirectiveType.Sdk,
                        "project" => FileBasedDirectiveType.Project,
                        _ => FileBasedDirectiveType.Unknown
                    };
                    yield return new IgnoredDirectiveWrapper(directive, ignoredDirectiveTriviaSyntax.Content.Text, trivia);
                }
            }
        }
    }
}


public class PackageDirectiveWrapper
{
    private readonly IgnoredDirectiveWrapper _wrapper;
    public PackageInfo? PackageInfo { get; set; }

    internal PackageDirectiveWrapper(IgnoredDirectiveWrapper wrapper)
    {
        if (wrapper.Directive != FileBasedDirectiveType.Package)
        {
            throw new ArgumentException("The provided wrapper does not contain a package directive");
        }
        _wrapper = wrapper;
        PackageInfo = PackageInfo.SafeParse(_wrapper.Content);
    }

    /// <summary>
    /// Updates the compilation unit syntax with the values based on the package directive
    /// </summary>
    /// <param name="compilationUnitSyntax"></param>
    /// <returns></returns>
    public CompilationUnitSyntax Update(CompilationUnitSyntax compilationUnitSyntax)
    {
        var syntaxTriviaList = SyntaxFactory.ParseLeadingTrivia($"#:{_wrapper.Content}{Environment.NewLine}");
        return compilationUnitSyntax.ReplaceTrivia(_wrapper.trivia, syntaxTriviaList);
    }
}

/// <summary>
/// Represents a NuGet package reference with its name and version information parsed from a file-based app directive.
/// </summary>
/// <param name="Name">The package name identifier.</param>
/// <param name="Version">The package version string.</param>
public partial record PackageInfo(string Name, string Version)
{
    [GeneratedRegex(@"^(?<packageName>.+)@(?<version>.+)$")]
    public static partial Regex PackageDirectiveRegex { get; }
        
    public static PackageInfo? SafeParse(string candidate)
    {
        if (PackageDirectiveRegex.Match(candidate) is {Success: true} match)
        {
            return new PackageInfo(match.Groups["packageName"].Value, match.Groups["version"].Value);
        }

        return null;
    }
}
