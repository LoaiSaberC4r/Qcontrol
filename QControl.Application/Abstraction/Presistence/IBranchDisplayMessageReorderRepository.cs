using QControl.Domain.Entities;

namespace QControl.Application.Abstraction.Presistence;

public interface IBranchDisplayMessageReorderRepository
{
    Task<BranchDisplayMessageReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchDisplayMessageReorderItem> items,
        CancellationToken cancellationToken);
}

public sealed record BranchDisplayMessageReorderItem(
    int MessageId,
    int DisplayOrder,
    byte[] RowVersion);

public sealed record BranchDisplayMessageReorderResult(
    BranchDisplayMessageReorderStatus Status,
    IReadOnlyList<BranchDisplayMessage> Messages);

public enum BranchDisplayMessageReorderStatus
{
    Success,
    NotFound,
    WrongBranch,
    DuplicateMessageId,
    DuplicateDisplayOrder,
    InvalidDisplayOrder,
    ConcurrencyConflict,
    OrderConflict
}
