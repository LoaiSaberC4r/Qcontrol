using QControl.Domain.Entities;

namespace QControl.Application.Abstraction.Presistence;

public interface IBranchVideoReorderRepository
{
    Task<BranchVideoReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchVideoReorderItem> items,
        CancellationToken cancellationToken);
}

public sealed record BranchVideoReorderItem(
    int VideoId,
    int DisplayOrder,
    byte[] RowVersion);

public sealed record BranchVideoReorderResult(
    BranchVideoReorderStatus Status,
    IReadOnlyList<BranchVideo> Videos);

public enum BranchVideoReorderStatus
{
    Success,
    NotFound,
    WrongBranch,
    DuplicateVideoId,
    DuplicateDisplayOrder,
    InvalidDisplayOrder,
    ConcurrencyConflict,
    OrderConflict
}
