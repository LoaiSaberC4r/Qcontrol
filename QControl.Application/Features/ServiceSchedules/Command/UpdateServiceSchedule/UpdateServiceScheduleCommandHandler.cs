using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.UpdateServiceSchedule;

internal sealed class UpdateServiceScheduleCommandHandler
    : ICommandHandler<UpdateServiceScheduleCommand, ServiceScheduleResponse>
{
    private readonly IWriteReadRepository<ServiceSchedule>
        _scheduleReadRepository;
    private readonly IWriteRepository<ServiceSchedule>
        _scheduleWriteRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceScheduleCommandHandler(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        IWriteRepository<ServiceSchedule> scheduleWriteRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _scheduleReadRepository = scheduleReadRepository
            ?? throw new ArgumentNullException(nameof(scheduleReadRepository));
        _scheduleWriteRepository = scheduleWriteRepository
            ?? throw new ArgumentNullException(nameof(scheduleWriteRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceReadRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceScheduleResponse>> Handle(
        UpdateServiceScheduleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.Unauthenticated",
                ServiceScheduleMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branchResult =
            await ServiceScheduleRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "Update",
                requireActive: true,
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceScheduleResponse>.Fail(branchResult.Errors);
        }

        var schedule = await _scheduleReadRepository.FirstOrDefaultAsync(
            new GetServiceScheduleForMutationSpec(
                request.BranchId,
                request.LeafServiceId),
            cancellationToken);

        if (schedule is null)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.ScheduleNotFound",
                ServiceScheduleMessages.ScheduleNotFound,
                ErrorType.NotFound));
        }

        var serviceError =
            await ServiceScheduleRuleChecks.ValidateEligibleServiceForMutationAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                "Update",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(serviceError);
        }

        string? normalizedSlotCode;
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots;

        try
        {
            normalizedSlotCode = ServiceScheduleSlotCodeNormalizer.Normalize(
                request.IsSlotCodeRequired,
                request.SlotCode);
            timeSlots = ServiceScheduleCommandMapper.ToDefinitions(
                request.Days);
        }
        catch (ArgumentException)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.InvalidSchedule",
                ServiceScheduleMessages.InvalidSchedule,
                ErrorType.Validation));
        }

        var duplicateSlotCodeError =
            await ServiceScheduleRuleChecks.ValidateSlotCodeIsUniqueAsync(
                _scheduleReadRepository,
                request.BranchId,
                normalizedSlotCode,
                schedule.Id,
                "Update",
                cancellationToken);

        if (duplicateSlotCodeError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(
                duplicateSlotCodeError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            schedule,
            rowVersion);

        try
        {
            schedule.Update(
                timeSlots,
                request.IsSlotCodeRequired,
                normalizedSlotCode,
                _currentUser.UserId.Value,
                _dateTimeProvider.UtcNow);
        }
        catch (ArgumentException)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.InvalidSchedule",
                ServiceScheduleMessages.InvalidSchedule,
                ErrorType.Validation));
        }

        _scheduleWriteRepository.Update(schedule);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.ConcurrencyConflict",
                ServiceScheduleMessages.ConcurrencyConflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException ex)
            when (ServiceScheduleUniqueConstraintErrorMapper.TryMapUpdate(
                ex,
                out var error))
        {
            return Result<ServiceScheduleResponse>.Fail(error);
        }

        var response =
            await ServiceScheduleRuleChecks.BuildResponseAsync(
                _scheduleReadRepository,
                _serviceReadRepository,
                _branchServiceReadRepository,
                branchResult.Value,
                request.LeafServiceId,
                schedule.Id,
                ServiceScheduleMessages.UpdateSuccess,
                cancellationToken);

        return response is null
            ? Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Update.NotFoundAfterSave",
                ServiceScheduleMessages.ScheduleNotFound,
                ErrorType.Infrastructure))
            : Result<ServiceScheduleResponse>.Ok(response);
    }
}
