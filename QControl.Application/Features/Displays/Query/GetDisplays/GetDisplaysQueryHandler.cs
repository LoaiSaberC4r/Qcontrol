using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplays;

internal sealed class GetDisplaysQueryHandler
    : IQueryHandler<GetDisplaysQuery, Pagination<DisplayListItemResponse>>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDisplaysQueryHandler(
        IWriteReadRepository<Display> displayReadRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<DisplayListItemResponse>>> Handle(
        GetDisplaysQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DisplayListItemResponse>>.Fail(
                new Error(
                    Code: "Displays.Pagination.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        request.Search ??= string.Empty;

        var specification = new GetDisplaysSpec(request);

        var (items, totalCount) =
            await _displayReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<DisplayListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<DisplayListItemResponse>>.Ok(response);
    }
}
