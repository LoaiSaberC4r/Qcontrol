using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Query.GetBranchDisplayMessages;

internal sealed class GetBranchDisplayMessagesQueryHandler
    : IQueryHandler<GetBranchDisplayMessagesQuery, IReadOnlyList<BranchDisplayMessageResponse>>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayMessage> _messages;
    private readonly ICurrentUser _currentUser;

    public GetBranchDisplayMessagesQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayMessage> messages,
        ICurrentUser currentUser)
    {
        _branches = branches;
        _messages = messages;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<BranchDisplayMessageResponse>>> Handle(
        GetBranchDisplayMessagesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayMessage.Get.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayMessage.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var messages = await _messages.ListAsync(
            new GetBranchDisplayMessagesSpec(request.BranchId),
            cancellationToken);
        return Result<IReadOnlyList<BranchDisplayMessageResponse>>.Ok(messages);
    }

    private static Result<IReadOnlyList<BranchDisplayMessageResponse>> Failure(
        string code, string message, ErrorType type) =>
        Result<IReadOnlyList<BranchDisplayMessageResponse>>.Fail(new Error(code, message, type));
}
