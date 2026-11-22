using System.Diagnostics;

namespace Worktree.Utils;

public union MatchOrError<T>(T, Error)
{
    public MatchOrError<U> Bind<U>(Func<T, MatchOrError<U>> next) => this switch
    {
        T success => next(success),
        Error error => error,
        _ => throw new UnreachableException()
    };
}

public static class MatchOrErrorExtensions
{
    extension<T>(Task<MatchOrError<T>> match)
    {
        public async Task<MatchOrError<U>> BindToMatchAsync<U>(Func<T, Task<MatchOrError<U>>> next)
        {
            var result = await match.ConfigureAwait(false);
            
            return result switch
            {
                T success => await next(success),
                Error error => error,
                _ => throw new UnreachableException()
            };
        }
        
        public async Task<MatchOrError<U>> BindToValueAsync<U>(Func<T, Task<U>> next)
        {
            var result = await match.ConfigureAwait(false);
            
            return result switch
            {
                T success => await next(success),
                Error error => error,
                _ => throw new UnreachableException()
            };
        }

      
        
    }
}