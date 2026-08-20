using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayConfiguration;

internal sealed class GetBranchDisplayConfigurationQueryHandler
    : IQueryHandler<GetBranchDisplayConfigurationQuery, BranchDisplayConfigurationResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<BranchDisplayConfiguration> _configurations;
    private readonly ICurrentUser _currentUser;

    public GetBranchDisplayConfigurationQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<BranchDisplayConfiguration> configurations,
        ICurrentUser currentUser)
    {
        _branches = branches;
        _configurations = configurations;
        _currentUser = currentUser;
    }

    public async Task<Result<BranchDisplayConfigurationResponse>> Handle(
        GetBranchDisplayConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure("BranchDisplayConfiguration.Get.Unauthenticated", ErrorMessage.Branch_Authentication_Required, ErrorType.Unauthorized);
        }
        if (!await _branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken))
        {
            return Failure("BranchDisplayConfiguration.BranchNotFound", ErrorMessage.Branch_NotFound, ErrorType.NotFound);
        }

        var response = await _configurations.FirstOrDefaultAsync(
            new GetBranchDisplayConfigurationSpec(request.BranchId),
            cancellationToken);
        return response is null
            ? Failure("BranchDisplayConfiguration.NotFound", BranchDisplayFeatureMessages.ConfigurationNotFound, ErrorType.NotFound)
            : Result<BranchDisplayConfigurationResponse>.Ok(response);
    }

    private static Result<BranchDisplayConfigurationResponse> Failure(
        string code,
        string message,
        ErrorType errorType) =>
        Result<BranchDisplayConfigurationResponse>.Fail(new Error(code, message, errorType));
}
