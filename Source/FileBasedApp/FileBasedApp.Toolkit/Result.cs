namespace FileBasedApp.Toolkit;

#if NET11_0_OR_GREATER
using System.Diagnostics;

/// <summary>
/// Represents successful completion of an operation that returns no value.
/// </summary>
public readonly struct Success;

/// <summary>
/// Represents either the value returned by a successful operation or the error that prevented it from succeeding.
/// </summary>
/// <typeparam name="TResult">The type of value produced on success.</typeparam>
/// <typeparam name="TError">The type of error reported on failure.</typeparam>
public union Result<TResult, TError>(TResult, TError);

/// <summary>
/// Represents either successful completion without a return value or an error.
/// </summary>
/// <typeparam name="TError">The type of error reported on failure.</typeparam>
public union Result<TError>(Success, TError);

/// <summary>
/// Composes result-producing operations and passes failures through unchanged.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Runs the next fallible operation on success or returns the original error.
    /// </summary>
    /// <typeparam name="TResult">The type of the input success value.</typeparam>
    /// <typeparam name="TError">The type of error shared by both operations.</typeparam>
    /// <typeparam name="TNext">The type of the next success value.</typeparam>
    /// <param name="result">The result to continue from.</param>
    /// <param name="next">The operation to run on success.</param>
    public static Result<TNext, TError> Bind<TResult, TError, TNext>(
        this Result<TResult, TError> result,
        Func<TResult, Result<TNext, TError>> next) => result switch
    {
        TResult success => next(success),
        TError error => error,
        _ => throw new UnreachableException()
    };

    /// <summary>
    /// Awaits the result and runs the next fallible operation only on success.
    /// </summary>
    /// <typeparam name="TResult">The type of the input success value.</typeparam>
    /// <typeparam name="TError">The type of error shared by both operations.</typeparam>
    /// <typeparam name="TNext">The type of the next success value.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="next">The asynchronous operation to run on success.</param>
    public static async Task<Result<TNext, TError>> BindToMatchAsync<TResult, TError, TNext>(
        this Task<Result<TResult, TError>> resultTask,
        Func<TResult, Task<Result<TNext, TError>>> next)
    {
        var result = await resultTask.ConfigureAwait(false);

        return result switch
        {
            TResult success => await next(success).ConfigureAwait(false),
            TError error => error,
            _ => throw new UnreachableException()
        };
    }

    /// <summary>
    /// Awaits the result and wraps the next operation's value as a success, or returns the original error.
    /// </summary>
    /// <typeparam name="TResult">The type of the input success value.</typeparam>
    /// <typeparam name="TError">The type of error carried by the result.</typeparam>
    /// <typeparam name="TNext">The type of the value produced by the next operation.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="next">The asynchronous operation to run on success.</param>
    public static async Task<Result<TNext, TError>> BindToValueAsync<TResult, TError, TNext>(
        this Task<Result<TResult, TError>> resultTask,
        Func<TResult, Task<TNext>> next)
    {
        var result = await resultTask.ConfigureAwait(false);

        return result switch
        {
            TResult success => await next(success).ConfigureAwait(false),
            TError error => error,
            _ => throw new UnreachableException()
        };
    }
}

#endif