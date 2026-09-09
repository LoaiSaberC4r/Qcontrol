using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Domain.Entities;

namespace QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;

internal sealed class GetTicketConfigurationQueryHandler
    : IQueryHandler<GetTicketConfigurationQuery, TicketConfigurationResponse>
{
    private const string CodePrefix = "TicketConfigurations.View";
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<TicketPrintConfiguration> _configurations;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccess;

    public GetTicketConfigurationQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<TicketPrintConfiguration> configurations,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccess)
    {
        _branches = branches;
        _configurations = configurations;
        _currentUser = currentUser;
        _branchAccess = branchAccess;
    }

    public async Task<Result<TicketConfigurationResponse>> Handle(
        GetTicketConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure($"{CodePrefix}.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }

        var access = _branchAccess.EnsureCanAccessBranch(request.BranchId, CodePrefix);
        if (access.IsFailure)
        {
            return Result<TicketConfigurationResponse>.Fail(access.Errors);
        }

        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure($"{CodePrefix}.BranchNotFound", ErrorMessage.Branch_NotFound,
                ErrorType.NotFound);
        }

        var configuration = await _configurations.FirstOrDefaultAsync(
            new GetTicketConfigurationSpec(request.BranchId), cancellationToken);
        return configuration is null
            ? Failure($"{CodePrefix}.NotFound", TicketConfigurationFeatureMessages.NotFound,
                ErrorType.NotFound)
            : Result<TicketConfigurationResponse>.Ok(configuration);
    }

    private static Result<TicketConfigurationResponse> Failure(
        string code, string message, ErrorType type) =>
        Result<TicketConfigurationResponse>.Fail(new Error(code, message, type));
}
