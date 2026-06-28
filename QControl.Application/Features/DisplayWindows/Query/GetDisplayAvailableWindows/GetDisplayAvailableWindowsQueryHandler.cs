using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

internal sealed class GetDisplayAvailableWindowsQueryHandler
    : IQueryHandler<GetDisplayAvailableWindowsQuery, Pagination<AvailableWindowResponse>>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDisplayAvailableWindowsQueryHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<AvailableWindowResponse>>> Handle(
        GetDisplayAvailableWindowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Failure(
                "DisplayWindows.Available.Unauthenticated",
                ErrorMessage.DisplayWindow_Authentication_Required,
                ErrorType.Security);
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayForAvailableWindowsSpec(request.DisplayId),
                cancellationToken);

        if (display is null)
        {
            return Failure(
                "DisplayWindows.Available.DisplayNotFound",
                ErrorMessage.DisplayWindow_Display_NotFound,
                ErrorType.NotFound);
        }

        if (display.IsDeleted)
        {
            return Failure(
                "DisplayWindows.Available.DisplayDeleted",
                ErrorMessage.DisplayWindow_Available_DisplayDeleted,
                ErrorType.Conflict);
        }

        request.Search ??= string.Empty;

        var specification = new GetDisplayAvailableWindowsSpec(
            request,
            display.BranchId);

        var (items, totalCount) =
            await _windowReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<AvailableWindowResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<AvailableWindowResponse>>.Ok(response);
    }

    private static Result<Pagination<AvailableWindowResponse>> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<Pagination<AvailableWindowResponse>>.Fail(
            new Error(
                Code: code,
                Message: message,
                Type: type));
}
