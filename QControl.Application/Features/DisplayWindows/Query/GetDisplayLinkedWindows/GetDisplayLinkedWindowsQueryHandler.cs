using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

internal sealed class GetDisplayLinkedWindowsQueryHandler
    : IQueryHandler<GetDisplayLinkedWindowsQuery, Pagination<DisplayLinkedWindowResponse>>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDisplayLinkedWindowsQueryHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(nameof(displayWindowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<DisplayLinkedWindowResponse>>> Handle(
        GetDisplayLinkedWindowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<DisplayLinkedWindowResponse>>.Fail(
                new Error(
                    Code: "DisplayWindows.Linked.Unauthenticated",
                    Message: ErrorMessage.DisplayWindow_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayForLinkedWindowsSpec(request.DisplayId),
                cancellationToken);

        if (display is null)
        {
            return Result<Pagination<DisplayLinkedWindowResponse>>.Fail(
                new Error(
                    Code: "DisplayWindows.Linked.DisplayNotFound",
                    Message: ErrorMessage.DisplayWindow_Display_NotFound,
                    Type: ErrorType.NotFound));
        }

        request.Search ??= string.Empty;

        var specification = new GetDisplayLinkedWindowsSpec(request);

        var (items, totalCount) =
            await _displayWindowReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<DisplayLinkedWindowResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<DisplayLinkedWindowResponse>>.Ok(response);
    }
}
