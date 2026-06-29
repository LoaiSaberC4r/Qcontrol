using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;

internal sealed class GetWaitingAreaByIdQueryHandler
    : IQueryHandler<GetWaitingAreaByIdQuery, WaitingAreaDetailsResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetWaitingAreaByIdQueryHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        ICurrentUser currentUser)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<WaitingAreaDetailsResponse>> Handle(
        GetWaitingAreaByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<WaitingAreaDetailsResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Details.Unauthenticated",
                    Message:
                        ErrorMessage.WaitingArea_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        var waitingArea =
            await _waitingAreaReadRepository.FirstOrDefaultAsync(
                new GetWaitingAreaByIdSpec(request.Id),
                cancellationToken);

        if (waitingArea is null)
        {
            return Result<WaitingAreaDetailsResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Details.WaitingAreaNotFound",
                    Message: ErrorMessage.WaitingArea_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<WaitingAreaDetailsResponse>.Ok(waitingArea);
    }
}
