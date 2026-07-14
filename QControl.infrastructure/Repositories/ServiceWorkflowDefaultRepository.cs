using System.Data;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed class ServiceWorkflowDefaultRepository
    : IServiceWorkflowDefaultRepository
{
    private readonly PlatformWriteDbContext _dbContext;

    public ServiceWorkflowDefaultRepository(
        PlatformWriteDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ServiceWorkflowSetDefaultResult> SetDefaultAsync(
        int branchId,
        int leafServiceId,
        int workflowId,
        byte[] rowVersion,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var candidate = await _dbContext.Set<ServiceWorkflow>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == workflowId)
            .Select(x => new
            {
                x.Id,
                x.BranchId,
                x.LeafServiceId,
                x.IsActive,
                x.IsDefault,
                x.RowVersion
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(ServiceWorkflowSetDefaultStatus.NotFound);
        }

        if (candidate.BranchId != branchId ||
            candidate.LeafServiceId != leafServiceId)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(ServiceWorkflowSetDefaultStatus.InvalidOwnership);
        }

        if (!candidate.IsActive)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(ServiceWorkflowSetDefaultStatus.Inactive);
        }

        if (!candidate.RowVersion.SequenceEqual(rowVersion))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(
                ServiceWorkflowSetDefaultStatus.ConcurrencyConflict);
        }

        if (candidate.IsDefault)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(ServiceWorkflowSetDefaultStatus.AlreadyDefault);
        }

        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
UPDATE [ServiceWorkflows]
SET [IsDefault] = CAST(
        CASE WHEN [Id] = {workflowId} THEN 1 ELSE 0 END AS bit),
    [LastModifiedByApplicationUserId] = {modifiedByApplicationUserId},
    [ModifiedOnUtc] = {modifiedOnUtc}
WHERE [BranchId] = {branchId}
  AND [LeafServiceId] = {leafServiceId}
  AND ([IsDefault] = CAST(1 AS bit) OR [Id] = {workflowId});",
            cancellationToken);

        if (affectedRows == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(
                ServiceWorkflowSetDefaultStatus.PersistenceConflict);
        }

        var updated = await _dbContext.Set<ServiceWorkflow>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x =>
                x.Id == workflowId &&
                x.BranchId == branchId &&
                x.LeafServiceId == leafServiceId)
            .FirstOrDefaultAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return updated is null
            ? Failure(ServiceWorkflowSetDefaultStatus.PersistenceConflict)
            : new ServiceWorkflowSetDefaultResult(
                ServiceWorkflowSetDefaultStatus.Success,
                updated);
    }

    private static ServiceWorkflowSetDefaultResult Failure(
        ServiceWorkflowSetDefaultStatus status)
        => new(status, Workflow: null);
}
