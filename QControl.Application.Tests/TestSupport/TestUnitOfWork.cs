using System.Data;
using System.Data.Common;
using BuildingBlock.Application.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using QControl.Application.Abstraction.Presistence;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public int BeginTransactionCallCount { get; private set; }

    public int CommitTransactionCallCount { get; private set; }

    public int RollbackTransactionCallCount { get; private set; }

    public IsolationLevel? LastIsolationLevel { get; private set; }

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
    {
        BeginTransactionCallCount++;

        return Task.FromResult<IDbContextTransaction>(
            new TestDbContextTransaction(this));
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken ct = default)
    {
        BeginTransactionCallCount++;
        LastIsolationLevel = isolationLevel;

        return Task.FromResult<IDbContextTransaction>(
            new TestDbContextTransaction(this));
    }

    public Task CommitTransactionAsync(CancellationToken ct = default)
    {
        CommitTransactionCallCount++;

        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        RollbackTransactionCallCount++;

        return Task.CompletedTask;
    }

    private sealed class TestDbContextTransaction
        : IDbContextTransaction
    {
        private readonly TestUnitOfWork _unitOfWork;

        public TestDbContextTransaction(TestUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Guid TransactionId { get; } = Guid.NewGuid();

        public bool SupportsSavepoints => false;

        public void Commit()
            => _unitOfWork.CommitTransactionCallCount++;

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            _unitOfWork.CommitTransactionCallCount++;

            return Task.CompletedTask;
        }

        public void Rollback()
            => _unitOfWork.RollbackTransactionCallCount++;

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            _unitOfWork.RollbackTransactionCallCount++;

            return Task.CompletedTask;
        }

        public DbTransaction GetDbTransaction()
            => throw new NotSupportedException();

        public void CreateSavepoint(string name)
            => throw new NotSupportedException();

        public Task CreateSavepointAsync(
            string name,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void RollbackToSavepoint(string name)
            => throw new NotSupportedException();

        public Task RollbackToSavepointAsync(
            string name,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void ReleaseSavepoint(string name)
            => throw new NotSupportedException();

        public Task ReleaseSavepointAsync(
            string name,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
