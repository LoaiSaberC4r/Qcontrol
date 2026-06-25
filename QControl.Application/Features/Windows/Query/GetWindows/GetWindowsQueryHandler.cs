using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Query.GetWindows;

internal sealed class GetWindowsQueryHandler
    : IQueryHandler<GetWindowsQuery, Pagination<WindowListItemResponse>>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetWindowsQueryHandler(
        IWriteReadRepository<Window> windowReadRepository,
        ICurrentUser currentUser)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<WindowListItemResponse>>> Handle(
        GetWindowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<WindowListItemResponse>>.Fail(
                new Error(
                    Code: "Windows.Pagination.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        request.Search ??= string.Empty;

        var specification = new GetWindowsSpec(request);

        var (items, totalCount) =
            await _windowReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<WindowListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<WindowListItemResponse>>.Ok(response);
    }
}
