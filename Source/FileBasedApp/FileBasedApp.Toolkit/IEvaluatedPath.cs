using TruePath;

namespace FileBasedApp.Toolkit;

/// <summary>
/// This contains information based on the evaluation of a path.
/// </summary>
public interface IEvaluatedPath
{
#if !NET11_0_OR_GREATER
    /// <summary>
    /// Evalutes and retur the given path or an error message
    /// </summary>
    /// <param name="shouldExist"></param>
    /// <param name="originalPathCanBeNull"></param>
    /// <returns>Returns a tuple containing the Path and an errormessage. If the error message is null the path is invalid</returns>
    (AbsolutePath path, string? errorMessage) GetPath(bool shouldExist, bool originalPathCanBeNull);
#else
    /// <summary>
    /// Evaluates the path and returns either an absolute path or an error message.
    /// </summary>
    /// <param name="shouldExist"></param>
    /// <param name="originalPathCanBeNull"></param>
    /// <returns></returns>
    PathOrError GetPath(bool shouldExist, bool originalPathCanBeNull);
    
#endif
    
    
}

#if NET11_0_OR_GREATER
/// <summary>
/// Represents either a valid absolute path or an error message describing why path evaluation failed.
/// </summary>
public union PathOrError(AbsolutePath, string)
{
    /// <summary>
    /// Returns true if the path is valid
    /// </summary>
    public bool Valid => this is AbsolutePath;

    /// <summary>
    /// Returns the valid path or throws an exception created from the error message.
    /// </summary>
    public AbsolutePath GetPathOrThrow<TException>(Func<string,TException> onError) where  TException : Exception
    {
        return this switch
        {
            AbsolutePath p => p,
            string s => throw onError(s)
        };
    }
    
}
#endif