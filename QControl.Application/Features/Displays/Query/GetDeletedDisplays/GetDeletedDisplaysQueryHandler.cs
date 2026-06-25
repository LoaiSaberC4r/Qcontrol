using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;

internal sealed class GetDeletedDisplaysQueryHandler
    : IQueryHandler<GetDeletedDisplaysQuery, Pagination<DeletedDisplayListItemResponse>>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDeletedDisplaysQueryHandler(
        IWriteReadRepository<Display> displayReadRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<DeletedDisplayListItemResponse>>> Handle(
        GetDeletedDisplaysQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DeletedDisplayListItemResponse>>.Fail(
                new Error(
                    Code: "Displays.DeletedPagination.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Security));
        }

        request.Search ??= string.Empty;

        var specification = new GetDeletedDisplaysSpec(request);

        var (items, totalCount) =
            await _displayReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<DeletedDisplayListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<DeletedDisplayListItemResponse>>.Ok(response);
    }
}
