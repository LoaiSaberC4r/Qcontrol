using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;

internal sealed class ReorderBranchVideosCommandHandler
    : ICommandHandler<ReorderBranchVideosCommand, IReadOnlyList<BranchVideoResponse>>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IBranchVideoReorderRepository _reorderRepository;
    private readonly ICurrentUser _currentUser;

    public ReorderBranchVideosCommandHandler(
        IWriteReadRepository<Branch> branches,
        IBranchVideoReorderRepository reorderRepository,
        ICurrentUser currentUser)
    {
        _branches = branches;
        _reorderRepository = reorderRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<BranchVideoResponse>>> Handle(
        ReorderBranchVideosCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchVideos.Reorder.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchVideos.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var persistenceItems = new List<BranchVideoReorderItem>(request.Items.Count);
        foreach (var item in request.Items)
        {
            if (!RowVersionConverter.TryDecode(item.RowVersion, out var rowVersion))
            {
                return Failure("BranchVideos.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
            }
            persistenceItems.Add(new BranchVideoReorderItem(item.VideoId, item.DisplayOrder, rowVersion));
        }

        var result = await _reorderRepository.ReorderAsync(
            request.BranchId,
            _currentUser.UserId.Value,
            persistenceItems,
            cancellationToken);
        if (result.Status != BranchVideoReorderStatus.Success)
        {
            return result.Status switch
            {
                BranchVideoReorderStatus.NotFound => Failure("BranchVideos.NotFound", BranchVideoMessages.NotFound, ErrorType.NotFound),
                BranchVideoReorderStatus.WrongBranch => Failure("BranchVideos.DoesNotBelongToBranch", BranchVideoMessages.OwnershipMismatch, ErrorType.Conflict),
                BranchVideoReorderStatus.DuplicateVideoId => Failure("BranchVideos.DuplicateVideoId", BranchVideoMessages.DuplicateVideoId, ErrorType.Validation),
                BranchVideoReorderStatus.DuplicateDisplayOrder => Failure("BranchVideos.DuplicateDisplayOrder", BranchVideoMessages.DuplicateDisplayOrder, ErrorType.Validation),
                BranchVideoReorderStatus.InvalidDisplayOrder => Failure("BranchVideos.InvalidDisplayOrder", BranchVideoMessages.InvalidDisplayOrder, ErrorType.Validation),
                BranchVideoReorderStatus.ConcurrencyConflict => Failure("BranchVideos.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict),
                _ => Failure("BranchVideos.OrderConflict", BranchVideoMessages.OrderConflict, ErrorType.Conflict)
            };
        }

        return Result<IReadOnlyList<BranchVideoResponse>>.Ok(
            result.Videos.Select(BranchVideoResponseFactory.FromEntity).ToList());
    }

    private static Result<IReadOnlyList<BranchVideoResponse>> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<IReadOnlyList<BranchVideoResponse>>.Fail(new Error(code, message, type));
}
