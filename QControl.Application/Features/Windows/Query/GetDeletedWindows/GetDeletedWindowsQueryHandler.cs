using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;

internal sealed class GetDeletedWindowsQueryHandler
    : IQueryHandler<GetDeletedWindowsQuery, Pagination<DeletedWindowListItemResponse>>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDeletedWindowsQueryHandler(
        IWriteReadRepository<Window> windowReadRepository,
        ICurrentUser currentUser)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<DeletedWindowListItemResponse>>> Handle(
        GetDeletedWindowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DeletedWindowListItemResponse>>.Fail(
                new Error(
                    Code: "Windows.DeletedPagination.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        request.Search ??= string.Empty;

        var specification = new GetDeletedWindowsSpec(request);

        var (items, totalCount) =
            await _windowReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<DeletedWindowListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<DeletedWindowListItemResponse>>.Ok(
            response);
    }
}
