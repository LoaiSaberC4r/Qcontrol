using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminals;

internal sealed class GetTerminalsQueryHandler
    : IQueryHandler<GetTerminalsQuery, Pagination<TerminalListItemResponse>>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetTerminalsQueryHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        ICurrentUser currentUser)
    {
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<TerminalListItemResponse>>> Handle(
        GetTerminalsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<TerminalListItemResponse>>.Fail(
                new Error(
                    Code: "Terminals.Pagination.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Security));
        }

        request.Search ??= string.Empty;

        var specification = new GetTerminalsSpec(request);

        var (items, totalCount) =
            await _terminalReadRepository.ListWithCountAsync(
                specification,
                cancellationToken);

        var response = new Pagination<TerminalListItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<TerminalListItemResponse>>.Ok(response);
    }
}
