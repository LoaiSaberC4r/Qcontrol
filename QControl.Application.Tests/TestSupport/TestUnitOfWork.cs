using System.Data;
using BuildingBlock.Application.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using QControl.Application.Abstraction.Presistence;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public IWriteRepository<TEntity, PlatformWriteMarker> WriteRepository<TEntity>()
        where TEntity : class
        => throw new NotSupportedException();

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCallCount++;

        return Task.FromResult(1);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task CommitTransactionAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task RollbackTransactionAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}
