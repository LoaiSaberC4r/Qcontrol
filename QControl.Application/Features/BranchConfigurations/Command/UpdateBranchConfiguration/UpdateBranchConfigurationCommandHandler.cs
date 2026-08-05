using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;

internal sealed class UpdateBranchConfigurationCommandHandler
    : ICommandHandler<
        UpdateBranchConfigurationCommand,
        BranchConfigurationResponse>
{
    private const string ErrorCodePrefix = "BranchConfigurations.Update";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchConfiguration>
        _configurationReadRepository;
    private readonly IWriteRepository<BranchConfiguration>
        _configurationWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchConfigurationCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchConfiguration> configurationReadRepository,
        IWriteRepository<BranchConfiguration> configurationWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _configurationReadRepository = configurationReadRepository
            ?? throw new ArgumentNullException(
                nameof(configurationReadRepository));
        _configurationWriteRepository = configurationWriteRepository
            ?? throw new ArgumentNullException(
                nameof(configurationWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<BranchConfigurationResponse>> Handle(
        UpdateBranchConfigurationCommand request,
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

        var validationError = ValidateRequest(request, out var rowVersion);
        if (validationError is not null)
        {
            return Result<BranchConfigurationResponse>.Fail(validationError);
        }

        var configuration =
            await _configurationReadRepository.FirstOrDefaultAsync(
                new GetBranchConfigurationForUpdateSpec(request.BranchId),
                cancellationToken);

        if (configuration is null)
        {
            return Result<BranchConfigurationResponse>.Fail(new Error(
                $"{ErrorCodePrefix}.ConfigurationNotFound",
                BranchConfigurationFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            configuration,
            rowVersion);

        configuration.UpdateAllowedTime(request.AllowedTime!.Value);
        _configurationWriteRepository.Update(configuration);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<BranchConfigurationResponse>.Fail(new Error(
                $"{ErrorCodePrefix}.ConcurrencyConflict",
                BranchConfigurationFeatureMessages.ConcurrencyConflict,
                ErrorType.Conflict));
        }

        return Result<BranchConfigurationResponse>.Ok(
            BranchConfigurationResponseFactory.FromEntity(
                configuration,
                BranchConfigurationFeatureMessages.Updated));
    }

    private static Error? ValidateRequest(
        UpdateBranchConfigurationCommand request,
        out byte[] rowVersion)
    {
        rowVersion = Array.Empty<byte>();

        if (!request.AllowedTime.HasValue)
        {
            return new Error(
                $"{ErrorCodePrefix}.AllowedTimeRequired",
                BranchConfigurationFeatureMessages.AllowedTimeRequired,
                ErrorType.Validation);
        }

        if (request.AllowedTime.Value < TimeSpan.Zero ||
            request.AllowedTime.Value >= TimeSpan.FromDays(1))
        {
            return new Error(
                $"{ErrorCodePrefix}.AllowedTimeOutOfRange",
                BranchConfigurationFeatureMessages.AllowedTimeOutOfRange,
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(request.RowVersion))
        {
            return new Error(
                $"{ErrorCodePrefix}.RowVersionRequired",
                BranchConfigurationFeatureMessages.RowVersionRequired,
                ErrorType.Validation);
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out rowVersion))
        {
            return new Error(
                $"{ErrorCodePrefix}.InvalidRowVersion",
                BranchConfigurationFeatureMessages.InvalidRowVersion,
                ErrorType.Validation);
        }

        return null;
    }
}
