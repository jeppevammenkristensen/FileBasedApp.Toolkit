using System.IO.Abstractions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TruePath;

namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Wraps a file-based app and provides access to its syntax tree representation.
/// Validates that the specified file is a valid file-based app during construction and parses its content into a Roslyn CompilationUnitSyntax.
/// </summary>
public class FileBasedAppWrapper
{
    /// <summary>
    /// Gets the absolute path to the file-based app source file.
    /// </summary>
    public AbsolutePath Path { get; private set; }

    /// <summary>
    /// Gets or sets the parsed Roslyn compilation unit syntax tree for the file-based app.
    /// </summary>
    public CompilationUnitSyntax CompilationUnitSyntax { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="FileBasedAppWrapper"/> by validating the file is a file-based app and parsing its content.
    /// </summary>
    /// <param name="path">The absolute path to the C# source file.</param>
    /// <param name="fileSystem">Optional file system abstraction for reading the file. Defaults to the real file system.</param>
    /// <exception cref="ArgumentException">Thrown when the file at <paramref name="path"/> is not a valid file-based app.</exception>
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

    /// <summary>
    /// Gets the package directives declared in the file-based app's leading trivia.
    /// </summary>
    public IEnumerable<PackageDirectiveWrapper> PackageDirectives => GetFileBasedDirectives()
        .Where(x => x.Directive == FileBasedDirectiveType.Package)
        .Select(x => new PackageDirectiveWrapper(x));

    /// <summary>
    /// Saves the current compilation unit syntax to the file system.
    /// </summary>
    /// <param name="fileSystem">Optional file system abstraction for writing the file. Defaults to the real file system.</param>
    public void Save(IFileSystem? fileSystem = null) => Save(CompilationUnitSyntax, fileSystem);
    
    /// <summary>
    /// Saves the specified compilation unit syntax to the file system and updates the current syntax tree representation.
    /// </summary>
    /// <param name="compilationUnitSyntax">The compilation unit syntax to save.</param>
    /// <param name="fileSystem">Optional file system abstraction for writing the file. Defaults to the real file system.</param>
    public void Save(CompilationUnitSyntax compilationUnitSyntax, IFileSystem? fileSystem = null)
    {
        fileSystem ??= new FileSystem();
        CompilationUnitSyntax = compilationUnitSyntax;
        fileSystem.File.WriteAllText(Path.Value, compilationUnitSyntax.ToFullString());
    }

}