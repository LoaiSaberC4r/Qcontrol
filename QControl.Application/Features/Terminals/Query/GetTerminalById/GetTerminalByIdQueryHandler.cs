using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminalById;

internal sealed class GetTerminalByIdQueryHandler
    : IQueryHandler<GetTerminalByIdQuery, TerminalDetailsResponse>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetTerminalByIdQueryHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        ICurrentUser currentUser)
    {
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<TerminalDetailsResponse>> Handle(
        GetTerminalByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<TerminalDetailsResponse>.Fail(
                new Error(
                    Code: "Terminals.Details.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        var terminal =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new GetTerminalByIdSpec(request.Id),
                cancellationToken);

        if (terminal is null)
        {
            return Result<TerminalDetailsResponse>.Fail(
                new Error(
                    Code: "Terminals.Details.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<TerminalDetailsResponse>.Ok(terminal);
    }
}
