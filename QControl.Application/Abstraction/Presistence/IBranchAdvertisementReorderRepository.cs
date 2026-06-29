using QControl.Domain.Entities;

namespace QControl.Application.Abstraction.Presistence;

public interface IBranchAdvertisementReorderRepository
{
    Task<BranchAdvertisementReorderResult> ReorderAsync(
        int branchId,
        Guid lastModifiedByApplicationUserId,
        IReadOnlyCollection<BranchAdvertisementReorderItem> items,
        CancellationToken cancellationToken);
}

public sealed record BranchAdvertisementReorderItem(
    int AdvertisementId,
    int DisplayOrder,
    byte[] RowVersion);

public sealed record BranchAdvertisementReorderResult(
    BranchAdvertisementReorderStatus Status,
    IReadOnlyList<BranchAdvertisement> Advertisements);

public enum BranchAdvertisementReorderStatus
{
    Success,
    Incomplete,
    DuplicateDisplayOrder,
    InvalidDisplayOrder,
    ConcurrencyConflict,
    OrderConflict
}
