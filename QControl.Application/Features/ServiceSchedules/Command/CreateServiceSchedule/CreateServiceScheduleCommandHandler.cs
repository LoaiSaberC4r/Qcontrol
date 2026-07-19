using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Command.CreateServiceSchedule;

internal sealed class CreateServiceScheduleCommandHandler
    : ICommandHandler<CreateServiceScheduleCommand, ServiceScheduleResponse>
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
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceScheduleCommandHandler(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        IWriteRepository<ServiceSchedule> scheduleWriteRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
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
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceScheduleResponse>> Handle(
        CreateServiceScheduleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Create.Unauthenticated",
                ServiceScheduleMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var branchResult =
            await ServiceScheduleRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "Create",
                requireActive: true,
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceScheduleResponse>.Fail(branchResult.Errors);
        }

        var serviceError =
            await ServiceScheduleRuleChecks.ValidateEligibleServiceForMutationAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                "Create",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(serviceError);
        }

        var duplicateScheduleError =
            await ServiceScheduleRuleChecks.ValidateScheduleDoesNotExistAsync(
                _scheduleReadRepository,
                request.BranchId,
                request.LeafServiceId,
                "Create",
                cancellationToken);

        if (duplicateScheduleError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(
                duplicateScheduleError);
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
                "ServiceSchedules.Create.InvalidSchedule",
                ServiceScheduleMessages.InvalidSchedule,
                ErrorType.Validation));
        }

        var duplicateSlotCodeError =
            await ServiceScheduleRuleChecks.ValidateSlotCodeIsUniqueAsync(
                _scheduleReadRepository,
                request.BranchId,
                normalizedSlotCode,
                excludedScheduleId: null,
                "Create",
                cancellationToken);

        if (duplicateSlotCodeError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(
                duplicateSlotCodeError);
        }

        ServiceSchedule schedule;

        try
        {
            schedule = ServiceSchedule.Create(
                request.BranchId,
                request.LeafServiceId,
                timeSlots,
                request.IsSlotCodeRequired,
                normalizedSlotCode,
                _currentUser.UserId.Value);
        }
        catch (ArgumentException)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Create.InvalidSchedule",
                ServiceScheduleMessages.InvalidSchedule,
                ErrorType.Validation));
        }

        await _scheduleWriteRepository.AddAsync(
            schedule,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ServiceScheduleUniqueConstraintErrorMapper.TryMapCreate(
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
                ServiceScheduleMessages.CreateSuccess,
                cancellationToken);

        return response is null
            ? Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Create.NotFoundAfterSave",
                ServiceScheduleMessages.ScheduleNotFound,
                ErrorType.Infrastructure))
            : Result<ServiceScheduleResponse>.Ok(response);
    }
}
