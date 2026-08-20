using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.UpdateBranchDisplayMessage;

internal sealed class UpdateBranchDisplayMessageCommandHandler
    : ICommandHandler<UpdateBranchDisplayMessageCommand, BranchDisplayMessageResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayMessage> _messages;
    private readonly IWriteRepository<BranchDisplayMessage> _writer;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchDisplayMessageCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayMessage> messages,
        IWriteRepository<BranchDisplayMessage> writer,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _messages = messages;
        _writer = writer;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchDisplayMessageResponse>> Handle(
        UpdateBranchDisplayMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayMessage.Update.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayMessage.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var message = await _messages.FirstOrDefaultAsync(
            new GetBranchDisplayMessageForMutationSpec(request.MessageId),
            cancellationToken);
        if (message is null)
        {
            return Failure("BranchDisplayMessage.NotFound", BranchDisplayFeatureMessages.MessageNotFound, ErrorType.NotFound);
        }
        if (message.BranchId != request.BranchId)
        {
            return Failure("BranchDisplayMessage.DoesNotBelongToBranch", BranchDisplayFeatureMessages.MessageOwnershipMismatch, ErrorType.Conflict);
        }
        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure("BranchDisplayMessage.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
        }
        if (await _messages.AnyAsync(
                x => x.BranchId == request.BranchId &&
                     x.Id != request.MessageId &&
                     x.DisplayOrder == request.DisplayOrder,
                cancellationToken))
        {
            return Failure("BranchDisplayMessage.OrderConflict", BranchDisplayFeatureMessages.OrderConflict, ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(message, rowVersion);
        message.Update(request.TextAr, request.TextEn, request.DisplayOrder, _currentUser.UserId.Value);
        _writer.Update(message);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure("BranchDisplayMessage.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict);
        }
        catch (DbUpdateException ex) when (
            BranchDisplayUniqueConstraintErrorMapper.TryMapMessageOrder(ex, out var error))
        {
            return Result<BranchDisplayMessageResponse>.Fail(error);
        }

        return Result<BranchDisplayMessageResponse>.Ok(
            BranchDisplayMessageResponseFactory.FromEntity(message));
    }

    private static Result<BranchDisplayMessageResponse> Failure(
        string code, string message, ErrorType type) =>
        Result<BranchDisplayMessageResponse>.Fail(new Error(code, message, type));
}
