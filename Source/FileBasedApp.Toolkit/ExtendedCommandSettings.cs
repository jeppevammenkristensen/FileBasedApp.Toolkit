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
    /// <param name="root">The path to use as root (in case the candidatePath is relative or empty)</param>
    /// <param name="allowEmpty">Allows the candidatePath to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exist</param>
    /// <param name="param"></param>
    /// <returns></returns>
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist, PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePath))] string? param = null)
    {
        return TryGetDirectory(candidatePath, allowEmpty, shouldExist, root.GetRootFolder(), param);
    }

    /// <summary>
    /// Tries to get a directory from a string
    /// </summary>
    /// <param name="candidatePath">The candidate to evaluate</param>
    /// <param name="allowEmpty">Allows the candidatePath to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exist</param>
    /// <param name="rootPaths">The paths to use as roots (in case the candidatePath is relative or empty)</param>
    /// <param name="paramName">Used in the exception thrown</param>
    /// <returns>The absolute path</returns>
    /// <remarks>If no paths are added. The current working directory is used as default root</remarks>
    /// <exception cref="InvalidOperationException">Throw an exception that can be caught by the do validate method</exception>
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist, [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null, params AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
        (AbsolutePath path, string? errorMessage) = result.GetPath(shouldExist, allowEmpty);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
        
    }

    /// <summary>
    /// Tries to get a directory from the candidate path
    /// </summary>
    /// <param name="candidatePath">The candidate path</param>
    /// <param name="root">The root folder to use in case the candidatePath is relative</param>
    /// <param name="allowEmpty">Allows the candidate to be empty</param>
    /// <param name="shouldExist">Evaluate if the directory should exists</param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist, AbsolutePath root, [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        return TryGetDirectory(candidatePath, allowEmpty, shouldExist, paramName, root);
    }
    
    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="root">The <see cref="AbsolutePath"/> representing the root path to use when the candidate is relative</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    protected AbsolutePath TryGetFile(string candidatePath, bool shouldExist, AbsolutePath root,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        return TryGetFile(candidatePath, shouldExist, paramName, root);
    }
    
    /// <summary>
    /// Attempts to resolve a file path based on the specified candidate path, predefined root, and existence requirements.
    /// </summary>
    /// <param name="candidatePath">The candidate file path provided by the user. It must not be null, empty, or whitespace.</param>
    /// <param name="shouldExist">Indicates whether the specified file must exist. If true, an exception will be thrown if the file does not exist.</param>
    /// <param name="paramName">The name of the parameter corresponding to <paramref name="candidatePath"/>. This is automatically supplied by the compiler via the <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="roots">The <see cref="AbsolutePath"/> representing the root path to use when the candidate is relative</param>
    /// <returns>An <see cref="AbsolutePath"/> representing the resolved file path.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="candidatePath"/> is null, empty, whitespace, or cannot be resolved to a valid file path.</exception>
    protected AbsolutePath TryGetFile(string candidatePath, bool shouldExist, [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null,
        params AbsolutePath[] roots)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"{paramName}: Candidate path cannot be empty");
        }

        var (path, errorMessage) = candidatePath.AnalyzeFile(roots).GetPath(shouldExist, false);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
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
    protected AbsolutePath TryGetFile(string candidatePath, bool shouldExist, PredefinedRootPath root,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        return TryGetFile(candidatePath, shouldExist, paramName, root.GetRootFolder());
    }
}