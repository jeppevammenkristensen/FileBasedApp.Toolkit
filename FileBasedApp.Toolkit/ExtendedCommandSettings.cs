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
    
    protected virtual void CustomValidation() {}
    

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

    protected AbsolutePath TryGetFile(string candidatePath, PredefinedRootPath root, bool shouldExist, [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"{paramName}: Candidate path cannot be empty");
        }
        
        return candidatePath.AnalyzeFile(root.GetRootFolder()).GetPath(shouldExist, false);
    }
}