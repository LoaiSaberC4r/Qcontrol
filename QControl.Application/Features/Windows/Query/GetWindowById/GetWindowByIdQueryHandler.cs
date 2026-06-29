using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Query.GetWindowById;

internal sealed class GetWindowByIdQueryHandler
    : IQueryHandler<GetWindowByIdQuery, WindowDetailsResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetWindowByIdQueryHandler(
        IWriteReadRepository<Window> windowReadRepository,
        ICurrentUser currentUser)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<WindowDetailsResponse>> Handle(
        GetWindowByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<WindowDetailsResponse>.Fail(
                new Error(
                    Code: "Windows.Details.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        var window =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetWindowByIdSpec(request.Id),
                cancellationToken);

        if (window is null)
        {
            return Result<WindowDetailsResponse>.Fail(
                new Error(
                    Code: "Windows.Details.WindowNotFound",
                    Message: ErrorMessage.Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<WindowDetailsResponse>.Ok(window);
    }
}
