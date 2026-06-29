using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;

internal sealed class ReorderBranchAdvertisementsCommandHandler
    : ICommandHandler<ReorderBranchAdvertisementsCommand, IReadOnlyList<BranchAdvertisementResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IBranchAdvertisementReorderRepository _reorderRepository;
    private readonly ICurrentUser _currentUser;

    public ReorderBranchAdvertisementsCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IBranchAdvertisementReorderRepository reorderRepository,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _reorderRepository = reorderRepository
            ?? throw new ArgumentNullException(nameof(reorderRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyList<BranchAdvertisementResponse>>> Handle(
        ReorderBranchAdvertisementsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.Reorder.Unauthenticated",
                    ErrorMessage.Branch_Authentication_Required,
                    ErrorType.Unauthorized));
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.BranchNotFound",
                    ErrorMessage.Branch_NotFound,
                    ErrorType.NotFound));
        }

        var persistenceItems =
            new List<BranchAdvertisementReorderItem>(request.Items.Count);

        foreach (var item in request.Items)
        {
            if (!RowVersionConverter.TryDecode(
                    item.RowVersion,
                    out var rowVersion))
            {
                return ValidationFailure(
                    "BranchAdvertisements.InvalidRowVersion",
                    ErrorMessage.RowVersion_Invalid);
            }

            persistenceItems.Add(new BranchAdvertisementReorderItem(
                item.AdvertisementId,
                item.DisplayOrder,
                rowVersion));
        }

        var result = await _reorderRepository.ReorderAsync(
            request.BranchId,
            _currentUser.UserId.Value,
            persistenceItems,
            cancellationToken);

        if (result.Status != BranchAdvertisementReorderStatus.Success)
        {
            return FailureFromStatus(result.Status);
        }

        return Result<IReadOnlyList<BranchAdvertisementResponse>>.Ok(
            result.Advertisements
                .Select(BranchAdvertisementResponseFactory.FromEntity)
                .ToList());
    }

    private static Result<IReadOnlyList<BranchAdvertisementResponse>>
        FailureFromStatus(BranchAdvertisementReorderStatus status)
    {
        return status switch
        {
            BranchAdvertisementReorderStatus.DuplicateDisplayOrder =>
                ValidationFailure(
                    "BranchAdvertisements.DuplicateDisplayOrder",
                    BranchFeatureMessages.DuplicateDisplayOrder),

            BranchAdvertisementReorderStatus.InvalidDisplayOrder =>
                ValidationFailure(
                    "BranchAdvertisements.InvalidDisplayOrder",
                    BranchFeatureMessages.InvalidDisplayOrder),

            BranchAdvertisementReorderStatus.ConcurrencyConflict =>
                Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                    new Error(
                        "BranchAdvertisements.ConcurrencyConflict",
                        ErrorMessage.Concurrency_Conflict,
                        ErrorType.Conflict)),

            BranchAdvertisementReorderStatus.OrderConflict =>
                Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                    new Error(
                        "BranchAdvertisements.OrderConflict",
                        BranchFeatureMessages.AdvertisementOrderConflict,
                        ErrorType.Conflict)),

            _ => ValidationFailure(
                "BranchAdvertisements.IncompleteReorder",
                BranchFeatureMessages.IncompleteReorder)
        };
    }

    private static Result<IReadOnlyList<BranchAdvertisementResponse>>
        ValidationFailure(string code, string message) =>
        Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
            new Error(code, message, ErrorType.Validation));
}
