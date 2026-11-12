using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using Spectre.Console;
using Spectre.Console.Cli;
using TruePath;

namespace FileBasedApp.Toolkit.CommandCli;

/// <summary>
/// Extends upon the existing <see cref="CommandSettings"/> to add convience
/// methods to aid with settings up a command line application with file based apps
/// </summary>
public abstract class ExtendedCommandSettings : CommandSettings
{
    /// <summary>
    /// Gets the deserializer used to load settings from files.
    /// </summary>
    /// <value>
    /// An <see cref="IDeserializer"/> instance used for deserializing file content.
    /// The default implementation returns a <see cref="JsonDeserializer"/>.
    /// Override this property to provide a custom deserializer implementation.
    /// </value>
    protected virtual IDeserializer Deserializer => new JsonDeserializer();

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
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePath))] string? param = null)
    {
        return TryGetDirectory(candidatePath, allowEmpty, shouldExist, root.GetRootFolder(), param);
    }

    /// <summary>
    /// Resolves directory candidates using a predefined root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to the root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="root">The predefined root used to resolve relative or empty candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    protected AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetDirectories(candidatePaths, allowEmpty, shouldExist, paramName, root.GetRootFolder());
    }

    /// <summary>
    /// Resolves directory candidates using an absolute root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The directory candidates. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to the root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="root">The root used to resolve relative or empty candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    protected AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        AbsolutePath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetDirectories(candidatePaths, allowEmpty, shouldExist, paramName, root);
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
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null, params AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
#if !NET11_0_OR_GREATER
        (AbsolutePath path, string? errorMessage) = result.GetPath(shouldExist, allowEmpty);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
#else
        return result.GetPath(shouldExist, allowEmpty).GetPathOrThrow(s => new InvalidOperationException(s));
#endif
    }

    /// <summary>
    /// Resolves directory paths against the supplied roots and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The candidate directory paths. An empty array is treated as a single empty candidate.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to a root directory.</param>
    /// <param name="shouldExist">Whether each resolved directory must exist.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <param name="rootPaths">The roots used to resolve relative or empty candidates; defaults to the current working directory.</param>
    /// <returns>The resolved absolute directory paths in candidate order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="candidatePaths"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when directory validation fails; errors for multiple candidates are aggregated.</exception>
    protected AbsolutePath[] TryGetDirectories(string[]? candidatePaths, bool allowEmpty, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null, params AbsolutePath[] rootPaths)
    {
        ArgumentNullException.ThrowIfNull(candidatePaths);

        if (candidatePaths.Length == 0)
        {
            return [TryGetDirectory(null, allowEmpty, shouldExist, paramName, rootPaths)];
        }

        if (candidatePaths.Length == 1)
        {
            return [TryGetDirectory(candidatePaths[0], allowEmpty, shouldExist, paramName, rootPaths)];
        }

        List<string> errors = [];
        List<AbsolutePath> paths = [];

        foreach (var candidatePath in candidatePaths)
        {
            var result = candidatePath.AnalyzeDirectory(rootPaths);
#if !NET11_0_OR_GREATER
            var (path, errorMessage) = result.GetPath(shouldExist, allowEmpty);
            if (errorMessage != null)
            {
                errors.Add(errorMessage);
            }
            else
            {
                paths.Add(path);
            }
#else
            var pathOrError = result.GetPath(shouldExist, allowEmpty);
            if (pathOrError is AbsolutePath path)
            {
                paths.Add(path);
            }
            else if (pathOrError is string error)
            {
                errors.Add(error);
            }
#endif
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"{paramName}: {errors.Count} directories: {string.Join(", ", errors)}");
        }

        return paths.ToArray();
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
    protected AbsolutePath TryGetDirectory(string? candidatePath, bool allowEmpty, bool shouldExist, AbsolutePath root,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null)
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
        [CallerArgumentExpression(nameof(candidatePath))]
        string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);

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
    protected AbsolutePath[] TryGetFiles(string[] candidatePath, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null,
        params AbsolutePath[] roots)
    {
        ArgumentNullException.ThrowIfNull(candidatePath);

        if (candidatePath.Length == 0)
        {
            throw new ArgumentException("Paths cannot be empty", nameof(candidatePath));
        }
        else if (candidatePath.Length == 1)
        {
            return [TryGetFile(candidatePath[0], shouldExist, paramName, roots)];
        }

        List<string> errors = [];
        List<AbsolutePath> paths = [];

        foreach (var s in candidatePath)
        {
#if !NET11_0_OR_GREATER
            var (path, errorMessage) = s.AnalyzeFile(roots).GetPath(shouldExist, false);
            if (errorMessage != null)
            {
                errors.Add(errorMessage);
            }
            else
            {
                paths.Add(path);
            }

#else
            var result = TryGetFileAsPathOrError(s, shouldExist);
            if (result is AbsolutePath path)
            {
                paths.Add(path);
            }
            else if (result is string error)
            {
                errors.Add(error);
            }
#endif
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"{errors.Count} files {string.Join(",", errors)}");
        }

        return paths.ToArray();
    }

    /// <summary>
    /// Resolves file candidates using a predefined root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The non-empty array of file candidates.</param>
    /// <param name="shouldExist">Whether each resolved file must exist.</param>
    /// <param name="root">The predefined root used to resolve relative candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute file paths in candidate order.</returns>
    protected AbsolutePath[] TryGetFiles(string[] candidatePaths, bool shouldExist,
        PredefinedRootPath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetFiles(candidatePaths, shouldExist, paramName, root.GetRootFolder());
    }

    /// <summary>
    /// Resolves file candidates using an absolute root and validates their existence when requested.
    /// </summary>
    /// <param name="candidatePaths">The non-empty array of file candidates.</param>
    /// <param name="shouldExist">Whether each resolved file must exist.</param>
    /// <param name="root">The root used to resolve relative candidates.</param>
    /// <param name="paramName">The caller's argument expression, used to identify validation errors.</param>
    /// <returns>The resolved absolute file paths in candidate order.</returns>
    protected AbsolutePath[] TryGetFiles(string[] candidatePaths, bool shouldExist,
        AbsolutePath root, [CallerArgumentExpression(nameof(candidatePaths))] string? paramName = null)
    {
        return TryGetFiles(candidatePaths, shouldExist, paramName, root);
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
    protected AbsolutePath TryGetFile(string candidatePath, bool shouldExist,
        [CallerArgumentExpression(nameof(candidatePath))] string? paramName = null,
        params AbsolutePath[] roots)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"{paramName}: Candidate path cannot be empty");
        }

#if !NET11_0_OR_GREATER
        var (path, errorMessage) = candidatePath.AnalyzeFile(roots).GetPath(shouldExist, false);
        if (errorMessage != null)
        {
            throw new InvalidOperationException($"{paramName}: {errorMessage}");
        }

        return path;
#else
        return candidatePath.AnalyzeFile()
            .GetPath(shouldExist, false).GetPathOrThrow(s => new InvalidOperationException($"{paramName}: {s}"));
#endif
    }

#if NET11_0_OR_GREATER
    /// <summary>
    /// Resolves a non-empty file candidate and returns either its absolute path or a validation error.
    /// </summary>
    /// <param name="candidatePath">The file path to resolve using the default roots.</param>
    /// <param name="shouldExist">Whether the resolved file must exist.</param>
    /// <returns>The resolved file path or an error describing why validation failed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="candidatePath"/> is null, empty, or whitespace.</exception>
    private PathOrError TryGetFileAsPathOrError(string candidatePath, bool shouldExist)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            throw new InvalidOperationException($"Candidate path cannot be empty");
        }

        return candidatePath.AnalyzeFile()
            .GetPath(shouldExist, false);
    }
    
    /// <summary>
    /// Resolves a directory candidate against the supplied roots, throwing if validation fails.
    /// </summary>
    /// <param name="candidatePath">The directory path to resolve, which may be empty when allowed.</param>
    /// <param name="shouldExist">Whether the resolved directory must exist.</param>
    /// <param name="allowEmpty">Whether an empty candidate may resolve to a root directory.</param>
    /// <param name="rootPaths">The roots used to resolve relative or empty candidates.</param>
    /// <returns>The resolved absolute directory path wrapped as a <see cref="PathOrError"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when directory validation fails.</exception>
    private PathOrError TryGetDirectoryAsPathOrError(string? candidatePath, bool shouldExist, bool allowEmpty, AbsolutePath[] rootPaths)
    {
        var result = candidatePath.AnalyzeDirectory(rootPaths);
        return result.GetPath(shouldExist, allowEmpty).GetPathOrThrow(s => new InvalidOperationException(s));
    }
    
#endif


    /// <summary>
    /// Retrieves a required value either from the provided input or from an environment variable.
    /// Throws an exception if the resulting value is null or whitespace.
    /// </summary>
    /// <param name="originalValue">The original value to check, which can be null or empty.</param>
    /// <param name="environmentKey">The key of the environment variable to fallback to if the original value is null or empty.</param>
    /// <param name="valueName">The name of the value being processed, primarily for use in exception messages. This is automatically captured by the compiler unless explicitly provided.</param>
    /// <returns>A non-empty string containing either the provided input or the value from the specified environment variable.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resulting value is null, empty, or whitespace.</exception>
    protected string? GetRequiredFromValueOrEnvironment(string? originalValue, string environmentKey,
        [CallerArgumentExpression(nameof(originalValue))]
        string? valueName = null)
    {
        var value = GetValueOrFromEnvironment(originalValue, environmentKey);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{valueName}: Value cannot be empty. ");
        }

        return value;
    }

    /// <summary>
    /// Loads and deserializes a settings object from a file at the specified path.
    /// The file is searched in the current working directory and execution folder if not found at the given path.
    /// Returns null if the path is null or whitespace.
    /// </summary>
    /// <param name="path">The path to the settings file to load. Can be relative or absolute.</param>
    /// <param name="fileSystem">Optional file system abstraction to use for file operations. If null, the default file system is used.</param>
    /// <typeparam name="T">The type to deserialize the settings into. Must be a reference type.</typeparam>
    /// <returns>The deserialized settings object of type T, or null if the path is null or whitespace.</returns>
    protected T? LoadSetting<T>(string? path, IFileSystem? fileSystem = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        var file = TryGetFile(path, true, roots: [PathUtil.GetCurrentWorkingFolder(), PathUtil.GetExecutionFolder()]);
        using var stream = file.OpenRead(fileSystem);
        return Deserializer.Deserialize<T>(stream);
    }

    /// <summary>
    /// Loads and deserializes a settings object from a file at the specified local path.
    /// The file is searched in the current working directory and execution folder.
    /// Returns null if the path is null.
    /// </summary>
    /// <typeparam name="T">The type of the settings object to deserialize. Must be a reference type.</typeparam>
    /// <param name="path">The local path to the settings file, or null if no file should be loaded.</param>
    /// <param name="fileSystem">Optional file system abstraction to use for file operations. If null, the default file system is used.</param>
    /// <returns>The deserialized settings object of type <typeparamref name="T"/>, or null if the path parameter is null.</returns>
    protected T? LoadSetting<T>(LocalPath? path, IFileSystem? fileSystem = null) where T : class
    {
        if (path == null) return null;

        var file = TryGetFile(path.Value.Value, true,
            roots: [PathUtil.GetCurrentWorkingFolder(), PathUtil.GetExecutionFolder()]);
        using var stream = file.OpenRead(fileSystem);
        return Deserializer.Deserialize<T>(stream);
    }

    /// <summary>
    /// Retrieves a non-empty value by either returning the provided value or falling back to the value of a specified environment variable.
    /// If the provided value is non-empty, it is returned as-is.
    /// Otherwise, the environment variable associated with the provided key is queried, and its value is returned.
    /// </summary>
    /// <param name="originalValue">The value to check and potentially return if it is non-empty.</param>
    /// <param name="environmentKey">The name of the environment variable to query if <paramref name="originalValue"/> is empty or null.</param>
    /// <returns>
    /// A string containing either the value of <paramref name="originalValue"/> if it is non-empty,
    /// or the value of the environment variable specified by <paramref name="environmentKey"/>.
    /// Returns null if both <paramref name="originalValue"/> and the environment variable are null or empty.
    /// </returns>
    protected string? GetValueOrFromEnvironment(string? originalValue, string environmentKey)
    {
        if (string.IsNullOrWhiteSpace(originalValue))
        {
            return Environment.GetEnvironmentVariable(environmentKey);
        }

        return originalValue;
    }
}
