using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Command.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.DeactivateBranchDisplayMessage;

internal sealed class DeactivateBranchDisplayMessageCommandHandler
    : ICommandHandler<DeactivateBranchDisplayMessageCommand, BranchDisplayMessageStateResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayMessage> _messages;
    private readonly IWriteRepository<BranchDisplayMessage> _writer;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchDisplayMessageCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayMessage> messages,
        IWriteRepository<BranchDisplayMessage> writer,
        IConcurrencyTokenManager concurrency,
        ICurrentUser currentUser,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _messages = messages;
        _writer = writer;
        _concurrency = concurrency;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchDisplayMessageStateResponse>> Handle(
        DeactivateBranchDisplayMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayMessage.Deactivate.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayMessage.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }
        var message = await _messages.FirstOrDefaultAsync(new GetBranchDisplayMessageForMutationSpec(request.MessageId), cancellationToken);
        if (message is null)
        {
            return Failure("BranchDisplayMessage.NotFound", BranchDisplayFeatureMessages.MessageNotFound, ErrorType.NotFound);
        }
        if (message.BranchId != request.BranchId)
        {
            return Failure("BranchDisplayMessage.DoesNotBelongToBranch", BranchDisplayFeatureMessages.MessageOwnershipMismatch, ErrorType.Conflict);
        }
        if (!message.IsActive)
        {
            return Failure("BranchDisplayMessage.AlreadyInactive", BranchDisplayFeatureMessages.MessageAlreadyInactive, ErrorType.Conflict);
        }
        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Failure("BranchDisplayMessage.InvalidRowVersion", ErrorMessage.RowVersion_Invalid, ErrorType.Validation);
        }

        _concurrency.SetOriginalRowVersion(message, rowVersion);
        message.Deactivate(_clock.UtcNow, _currentUser.UserId.Value);
        _writer.Update(message);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure("BranchDisplayMessage.ConcurrencyConflict", ErrorMessage.Concurrency_Conflict, ErrorType.Conflict);
        }

        return Result<BranchDisplayMessageStateResponse>.Ok(
            BranchDisplayMessageResponseFactory.StateFromEntity(message, BranchDisplayFeatureMessages.MessageDeactivated));
    }

    private static Result<BranchDisplayMessageStateResponse> Failure(
        string code, string message, ErrorType type) =>
        Result<BranchDisplayMessageStateResponse>.Fail(new Error(code, message, type));
}
