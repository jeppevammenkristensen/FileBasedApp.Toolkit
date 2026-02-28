using System.Runtime.CompilerServices;
using Spectre.Console;
using Spectre.Console.Cli;
using TruePath;

namespace FileBasedApp.Toolkit;



/// <summary>
/// Extends upon the existing <see cref="CommandSettings"/> to add convience
/// methods to aid with settings up a command line application with file based apps
/// </summary>
public abstract class ExtendedCommandSettings : CommandSettings
{
    /// <summary>
    /// Validates the current settings instance using custom validation logic.
    /// If an exception is thrown during validation, its message will be used as the error description in the result.
    /// </summary>
    /// <returns>A <see cref="ValidationResult"/> object indicating whether the validation succeeded or failed.</returns>
    public sealed override ValidationResult Validate()
    {
        try
        {
            return DoValidate();
        }
        catch (Exception e)
        {
            return ValidationResult.Error(e.Message);
        }
    }

    /// <summary>
    /// When implementing this you can return a validation result
    /// but be aware that you can also rely on exceptions being thrown. The message of those
    /// exception will be returned as validation errors
    /// /// </summary>
    /// <returns></returns>
    protected virtual ValidationResult DoValidate()
    {
        return ValidationResult.Success();
    }

    /// <summary>
    /// Tries to get a directory from a string
    /// </summary>
    /// <param name="candidatePath">The candidate to evaluate</param>
    /// <param name="root">The path to use as root (in cased the path is relative)</param>
    /// <param name="allowEmpty">Allows the candidate to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exist</param>
    /// <returns></returns>
    protected AbsolutePath TryGetDirectory(string? candidatePath, PredefinedRootPath root, bool allowEmpty, bool shouldExist)
    {
        var result = candidatePath.AnalyzeDirectory(root.GetRootFolder());
        return result.GetPath(shouldExist, allowEmpty);
    }

    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="root">The predefined root path that provides the folder context for resolving the candidate path.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    protected AbsolutePath TryGetFile(string candidatePath, PredefinedRootPath root, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"{paramName}: Candidate path cannot be empty");
        }
        
        return candidatePath.AnalyzeFile(root.GetRootFolder()).GetPath(shouldExist, false);
    }
}