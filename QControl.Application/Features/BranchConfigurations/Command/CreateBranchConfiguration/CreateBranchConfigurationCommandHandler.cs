using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;

internal sealed class CreateBranchConfigurationCommandHandler
    : ICommandHandler<
        CreateBranchConfigurationCommand,
        BranchConfigurationResponse>
{
    private const string ErrorCodePrefix = "BranchConfigurations.Create";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchConfiguration>
        _configurationReadRepository;
    private readonly IWriteRepository<BranchConfiguration>
        _configurationWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchConfigurationCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchConfiguration> configurationReadRepository,
        IWriteRepository<BranchConfiguration> configurationWriteRepository,
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
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<BranchConfigurationResponse>> Handle(
        CreateBranchConfigurationCommand request,
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

        var allowedTimeError = ValidateAllowedTime(request.AllowedTime);
        if (allowedTimeError is not null)
        {
            return Result<BranchConfigurationResponse>.Fail(
                allowedTimeError);
        }

        var alreadyExists = await _configurationReadRepository.AnyAsync(
            x => x.BranchId == request.BranchId,
            cancellationToken);

        if (alreadyExists)
        {
            return AlreadyExists();
        }

        var configuration = BranchConfiguration.Create(
            request.BranchId,
            request.AllowedTime!.Value,
            request.MaximumTicketCallAttempts,
            request.TicketNoShowAutoCancellationMinutes,
            request.TicketArchiveRetentionDays);

        await _configurationWriteRepository.AddAsync(
            configuration,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BranchConfigurationUniqueConstraintErrorMapper.TryMap(
                ex,
                out var error))
        {
            return Result<BranchConfigurationResponse>.Fail(error);
        }

        return Result<BranchConfigurationResponse>.Ok(
            BranchConfigurationResponseFactory.FromEntity(
                configuration,
                BranchConfigurationFeatureMessages.Created));
    }

    private static Error? ValidateAllowedTime(TimeSpan? allowedTime)
    {
        if (!allowedTime.HasValue)
        {
            return new Error(
                $"{ErrorCodePrefix}.AllowedTimeRequired",
                BranchConfigurationFeatureMessages.AllowedTimeRequired,
                ErrorType.Validation);
        }

        if (allowedTime.Value < TimeSpan.Zero ||
            allowedTime.Value >= TimeSpan.FromDays(1))
        {
            return new Error(
                $"{ErrorCodePrefix}.AllowedTimeOutOfRange",
                BranchConfigurationFeatureMessages.AllowedTimeOutOfRange,
                ErrorType.Validation);
        }

        return null;
    }

    private static Result<BranchConfigurationResponse> AlreadyExists() =>
        Result<BranchConfigurationResponse>.Fail(new Error(
            $"{ErrorCodePrefix}.AlreadyExists",
            BranchConfigurationFeatureMessages.AlreadyExists,
            ErrorType.Conflict));
}
