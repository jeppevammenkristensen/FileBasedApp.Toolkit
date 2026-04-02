namespace FileBasedApp.Toolkit.CSharp;

/// <summary>
/// Specifies the type of file-based directive found in C# source code trivia.
/// Represents directives that configure package references, properties, SDK versions, and project settings
/// in file-based applications.
/// </summary>
public enum FileBasedDirectiveType
{
    /// <summary>
    /// Indicates a package directive that declares a NuGet package dependency.
    /// Used to identify directives in the format #package [PackageName] [Version] in C# source files.
    /// </summary>
    Package,

    /// <summary>
    /// Represents a property directive that defines build configuration properties.
    /// This directive type is used to specify MSBuild properties that affect compilation and build behavior
    /// in file-based C# applications.
    /// </summary>
    Property,

    /// <summary>
    /// Represents a directive that specifies the SDK version for the file-based application.
    /// This directive type is used to configure which .NET SDK should be used for compilation and execution.
    /// </summary>
    Sdk,

    /// <summary>
    /// 
    /// </summary>
    Project,

    /// <summary>
    /// Indicates an unrecognized or invalid directive type that does not match any of the known file-based directive formats.
    /// Used as a fallback when a directive cannot be parsed as Package, Property, Sdk, or Project.
    /// </summary>
    Unknown
}