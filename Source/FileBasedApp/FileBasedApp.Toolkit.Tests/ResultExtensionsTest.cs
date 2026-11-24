#if NET11_0_OR_GREATER
using System;
using System.Threading.Tasks;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>
/// Verifies that result chains transform successes and stop at the first error.
/// </summary>
public class ResultExtensionsTest
{
    /// <summary>
    /// Verifies that synchronous binding passes the success value to the next operation.
    /// </summary>
    [Fact]
    public void Bind_ContinuesOnSuccess()
    {
        Result<int, string> input = 21;

        var result = input.Bind(value => (Result<long, string>) (value * 2L));

        Assert.True(result is 42L);
    }

    /// <summary>
    /// Verifies that synchronous binding passes through an error without invoking the next operation.
    /// </summary>
    [Fact]
    public void Bind_SkipsFailure()
    {
        Result<int, string> input = "original error";

        static Result<long, string> UnexpectedContinuation(int _) => throw new InvalidOperationException();

        var result = input.Bind(UnexpectedContinuation);

        Assert.True(result is "original error");
    }

    /// <summary>
    /// Verifies that both asynchronous variants pass successful values through the chain.
    /// </summary>
    [Fact]
    public async Task BindAsync_ContinuesOnSuccess()
    {
        Result<int, string> input = 20;

        var result = await Task.FromResult(input)
            .BindToMatchAsync(value => Task.FromResult((Result<long, string>) (value + 1L)))
            .BindToValueAsync(value => Task.FromResult(value * 2));

        Assert.True(result is long and 42L);
    }

    /// <summary>
    /// Verifies that both asynchronous variants skip their continuations when the input is an error.
    /// </summary>
    [Fact]
    public async Task BindAsync_SkipsFailure()
    {
        Result<int, string> input = "original error";

        var result = await Task.FromResult(input)
            .BindToMatchAsync(_ => Task.FromException<Result<long, string>>(new InvalidOperationException()))
            .BindToValueAsync(_ => Task.FromException<double>(new InvalidOperationException()));

        Assert.True(result is string and "original error");
    }

    /// <summary>
    /// Verifies that a new error from an asynchronous operation stops the rest of the chain.
    /// </summary>
    [Fact]
    public async Task BindAsync_PropagatesContinuationFailure()
    {
        Result<int, string> input = 21;

        var result = await Task.FromResult(input)
            .BindToMatchAsync(_ => Task.FromResult((Result<long, string>) "new error"))
            .BindToValueAsync(_ => Task.FromException<double>(new InvalidOperationException()));

        Assert.True(result is string and "new error");
    }
}

#endif