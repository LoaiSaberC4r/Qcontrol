using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

internal sealed class GetWaitingAreasQueryHandler
    : IQueryHandler<GetWaitingAreasQuery, Pagination<WaitingAreaListItemResponse>>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetWaitingAreasQueryHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        ICurrentUser currentUser)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<WaitingAreaListItemResponse>>> Handle(
        GetWaitingAreasQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<WaitingAreaListItemResponse>>.Fail(
                new Error(
                    Code: "WaitingAreas.Pagination.Unauthenticated",
                    Message:
                        ErrorMessage.WaitingArea_Authentication_Required,
                    Type: ErrorType.Security));
        }

        request.Search ??= string.Empty;

        var specification = new GetWaitingAreasSpec(request);

        var (items, totalCount) =
            await _waitingAreaReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<WaitingAreaListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<WaitingAreaListItemResponse>>.Ok(
            response);
    }
}
