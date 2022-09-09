using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Rinkudesu.Services.Links.Data;

public static class LinkDbContextExtensions
{
    public static async Task<TResult> ExecuteInTransaction<TState, TResult>(this LinkDbContext context, TState state, Func<TState, CancellationToken, Task<TResult>> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteInTransactionAsync(state, action, (_, _) => Task.FromResult(false), isolationLevel, cancellationToken).ConfigureAwait(false);
    }
}
