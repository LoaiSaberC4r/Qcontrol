using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplayById;

internal sealed class GetDisplayByIdQueryHandler
    : IQueryHandler<GetDisplayByIdQuery, DisplayDetailsResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDisplayByIdQueryHandler(
        IWriteReadRepository<Display> displayReadRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<DisplayDetailsResponse>> Handle(
        GetDisplayByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<DisplayDetailsResponse>.Fail(
                new Error(
                    Code: "Displays.Details.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayByIdSpec(request.Id),
                cancellationToken);

        if (display is null)
        {
            return Result<DisplayDetailsResponse>.Fail(
                new Error(
                    Code: "Displays.Details.DisplayNotFound",
                    Message: ErrorMessage.Display_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<DisplayDetailsResponse>.Ok(display);
    }
}
