using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

internal sealed class PermanentDeleteTerminalCommandHandler
    : ICommandHandler<PermanentDeleteTerminalCommand, PermanentDeleteTerminalResponse>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly ITerminalPermanentDeleteRepository _permanentDeleteRepository;
    private readonly ICurrentUser _currentUser;

    public PermanentDeleteTerminalCommandHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        ITerminalPermanentDeleteRepository permanentDeleteRepository,
        ICurrentUser currentUser)
    {
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _permanentDeleteRepository = permanentDeleteRepository
            ?? throw new ArgumentNullException(nameof(permanentDeleteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<PermanentDeleteTerminalResponse>> Handle(
        PermanentDeleteTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.PermanentDelete.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var terminal =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new GetTerminalForPermanentDeleteSpec(request.Id),
                cancellationToken);

        if (terminal is null)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.PermanentDelete.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        if (!terminal.IsDeleted)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.PermanentDelete.MustBeSoftDeleted",
                    Message:
                        ErrorMessage
                            .Terminal_PermanentDelete_MustBeSoftDeleted,
                    Type: ErrorType.Conflict));
        }

        int deletedRows;
        try
        {
            deletedRows =
                await _permanentDeleteRepository.DeletePermanentlyAsync(
                    request.Id,
                    cancellationToken);
        }
        catch (TerminalPermanentDeleteConflictException)
        {
            return HasRelatedRecords();
        }

        if (deletedRows == 0)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.PermanentDelete.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<PermanentDeleteTerminalResponse>.Ok(
            new PermanentDeleteTerminalResponse
            {
                Id = request.Id,
                Message = ErrorMessage.Terminal_PermanentDelete_Success
            });
    }

    private static Result<PermanentDeleteTerminalResponse> HasRelatedRecords()
        => Result<PermanentDeleteTerminalResponse>.Fail(
            new Error(
                Code: "Terminals.PermanentDelete.HasRelatedRecords",
                Message:
                    ErrorMessage
                        .Terminal_PermanentDelete_HasRelatedRecords,
                Type: ErrorType.Conflict));
}
