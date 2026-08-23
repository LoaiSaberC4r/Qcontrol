using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;

internal sealed class GetBranchConfigurationQueryHandler
    : IQueryHandler<GetBranchConfigurationQuery, BranchConfigurationResponse>
{
    private const string ErrorCodePrefix = "BranchConfigurations.View";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchConfiguration>
        _configurationReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetBranchConfigurationQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchConfiguration> configurationReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _configurationReadRepository = configurationReadRepository
            ?? throw new ArgumentNullException(
                nameof(configurationReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
    }

    public async Task<Result<BranchConfigurationResponse>> Handle(
        GetBranchConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchConfigurationResponse>.Fail(new Error(
                $"{ErrorCodePrefix}.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var accessResult = _branchAccessValidator.EnsureCanAccessBranch(
            request.BranchId,
            ErrorCodePrefix);

        if (accessResult.IsFailure)
        {
            return Result<BranchConfigurationResponse>.Fail(
                accessResult.Errors);
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<BranchConfigurationResponse>.Fail(new Error(
                $"{ErrorCodePrefix}.BranchNotFound",
                BranchConfigurationFeatureMessages.BranchNotFound,
                ErrorType.NotFound));
        }

        var configuration =
            await _configurationReadRepository.FirstOrDefaultAsync(
                new GetBranchConfigurationSpec(request.BranchId),
                cancellationToken);

        return Result<BranchConfigurationResponse>.Ok(
            configuration ??
            BranchConfigurationResponseFactory.NotConfigured(
                request.BranchId));
    }
}
