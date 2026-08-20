using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.CreateBranchDisplayMessage;

internal sealed class CreateBranchDisplayMessageCommandHandler
    : ICommandHandler<CreateBranchDisplayMessageCommand, BranchDisplayMessageResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayMessage> _messages;
    private readonly IWriteRepository<BranchDisplayMessage> _writer;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchDisplayMessageCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayMessage> messages,
        IWriteRepository<BranchDisplayMessage> writer,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _messages = messages;
        _writer = writer;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BranchDisplayMessageResponse>> Handle(
        CreateBranchDisplayMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayMessage.Create.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayMessage.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }
        if (await _messages.AnyAsync(
                x => x.BranchId == request.BranchId && x.DisplayOrder == request.DisplayOrder,
                cancellationToken))
        {
            return Failure("BranchDisplayMessage.OrderConflict", BranchDisplayFeatureMessages.OrderConflict, ErrorType.Conflict);
        }

        var message = BranchDisplayMessage.Create(
            request.BranchId,
            request.TextAr,
            request.TextEn,
            request.DisplayOrder,
            _currentUser.UserId.Value);
        await _writer.AddAsync(message, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
