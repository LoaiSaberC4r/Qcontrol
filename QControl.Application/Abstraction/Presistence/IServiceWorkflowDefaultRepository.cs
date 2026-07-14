using QControl.Domain.Entities;

namespace QControl.Application.Abstraction.Presistence;

public interface IServiceWorkflowDefaultRepository
{
    Task<ServiceWorkflowSetDefaultResult> SetDefaultAsync(
        int branchId,
        int leafServiceId,
        int workflowId,
        byte[] rowVersion,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc,
        CancellationToken cancellationToken);
}

public sealed record ServiceWorkflowSetDefaultResult(
    ServiceWorkflowSetDefaultStatus Status,
    ServiceWorkflow? Workflow);

public enum ServiceWorkflowSetDefaultStatus
{
    Success,
    NotFound,
    Inactive,
    ConcurrencyConflict,
    InvalidOwnership,
    AlreadyDefault,
    PersistenceConflict
}
