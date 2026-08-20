using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.ReorderBranchDisplayMessages;

internal sealed class ReorderBranchDisplayMessagesCommandHandler
    : ICommandHandler<ReorderBranchDisplayMessagesCommand, IReadOnlyList<BranchDisplayMessageResponse>>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IBranchDisplayMessageReorderRepository _reorderRepository;
    private readonly ICurrentUser _currentUser;

    public ReorderBranchDisplayMessagesCommandHandler(
        IWriteReadRepository<Branch> branches,
        IBranchDisplayMessageReorderRepository reorderRepository,
        ICurrentUser currentUser)
    {
        _branches = branches;
        _reorderRepository = reorderRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<BranchDisplayMessageResponse>>> Handle(
        ReorderBranchDisplayMessagesCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayMessage.Reorder.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayMessage.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var persistenceItems = new List<BranchDisplayMessageReorderItem>(request.Items.Count);
        foreach (var item in request.Items)
        {
            if (!RowVersionConverter.TryDecode(item.RowVersion, out var rowVersion))
            {
                return Failure("BranchDisplayMessage.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
            }
            persistenceItems.Add(new BranchDisplayMessageReorderItem(item.MessageId, item.DisplayOrder, rowVersion));
        }

        var result = await _reorderRepository.ReorderAsync(
            request.BranchId,
            _currentUser.UserId.Value,
            persistenceItems,
            cancellationToken);
        if (result.Status != BranchDisplayMessageReorderStatus.Success)
        {
            return result.Status switch
            {
                BranchDisplayMessageReorderStatus.NotFound => Failure("BranchDisplayMessage.NotFound", BranchDisplayFeatureMessages.MessageNotFound, ErrorType.NotFound),
                BranchDisplayMessageReorderStatus.WrongBranch => Failure("BranchDisplayMessage.DoesNotBelongToBranch", BranchDisplayFeatureMessages.MessageOwnershipMismatch, ErrorType.Conflict),
                BranchDisplayMessageReorderStatus.DuplicateMessageId => Failure("BranchDisplayMessage.DuplicateMessageId", BranchDisplayFeatureMessages.DuplicateMessageId, ErrorType.Validation),
                BranchDisplayMessageReorderStatus.DuplicateDisplayOrder => Failure("BranchDisplayMessage.DuplicateDisplayOrder", BranchDisplayFeatureMessages.DuplicateDisplayOrder, ErrorType.Validation),
                BranchDisplayMessageReorderStatus.InvalidDisplayOrder => Failure("BranchDisplayMessage.InvalidDisplayOrder", BranchDisplayFeatureMessages.InvalidDisplayOrder, ErrorType.Validation),
                BranchDisplayMessageReorderStatus.ConcurrencyConflict => Failure("BranchDisplayMessage.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict),
                _ => Failure("BranchDisplayMessage.OrderConflict", BranchDisplayFeatureMessages.OrderConflict, ErrorType.Conflict)
            };
        }

        return Result<IReadOnlyList<BranchDisplayMessageResponse>>.Ok(
            result.Messages.Select(BranchDisplayMessageResponseFactory.FromEntity).ToList());
    }

    private static Result<IReadOnlyList<BranchDisplayMessageResponse>> Failure(
        string code, string message, ErrorType type) =>
        Result<IReadOnlyList<BranchDisplayMessageResponse>>.Fail(new Error(code, message, type));
}
