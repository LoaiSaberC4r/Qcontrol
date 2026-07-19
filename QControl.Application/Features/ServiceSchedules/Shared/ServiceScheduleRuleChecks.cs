using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

internal static class ServiceScheduleRuleChecks
{
    public static async Task<Result<BranchScheduleState>>
        ValidateBranchAccessAndStateAsync(
            IWriteReadRepository<Branch> branchReadRepository,
            IBranchAccessValidator branchAccessValidator,
            int branchId,
            string operation,
            bool requireActive,
            CancellationToken cancellationToken)
    {
        var access = branchAccessValidator.EnsureCanAccessBranch(
            branchId,
            $"ServiceSchedules.{operation}");

        if (access.IsFailure)
        {
            return Result<BranchScheduleState>.Fail(access.Errors);
        }

        var branch = await branchReadRepository.FirstOrDefaultAsync(
            new GetBranchScheduleStateSpec(branchId),
            cancellationToken);

        if (branch is null)
        {
            return Result<BranchScheduleState>.Fail(new Error(
                $"ServiceSchedules.{operation}.BranchNotFound",
                ServiceScheduleMessages.BranchNotFound,
                ErrorType.NotFound));
        }

        if (requireActive && !branch.IsActive)
        {
            return Result<BranchScheduleState>.Fail(new Error(
                $"ServiceSchedules.{operation}.BranchInactive",
                ServiceScheduleMessages.BranchInactive,
                ErrorType.Conflict));
        }

        return Result<BranchScheduleState>.Ok(branch);
    }

    public static async Task<Error?> ValidateEligibleServiceForMutationAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        int branchId,
        int serviceId,
        string operation,
        CancellationToken cancellationToken)
    {
        var context = await LoadServiceEligibilityContextAsync(
            serviceReadRepository,
            branchServiceReadRepository,
            branchId,
            cancellationToken);

        return ValidateEligibleService(
            serviceId,
            branchId,
            context.ServicesById,
            context.ServiceStates,
            context.AssignedServiceIds,
            operation);
    }

    public static async Task<Error?> ValidateScheduleDoesNotExistAsync(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        int branchId,
        int serviceId,
        string operation,
        CancellationToken cancellationToken)
    {
        var existingId = await scheduleReadRepository.FirstOrDefaultAsync(
            new ServiceScheduleExistsSpec(branchId, serviceId),
            cancellationToken);

        return existingId > 0
            ? new Error(
                $"ServiceSchedules.{operation}.ScheduleAlreadyExists",
                ServiceScheduleMessages.ScheduleAlreadyExists,
                ErrorType.Conflict)
            : null;
    }

    public static async Task<Error?> ValidateSlotCodeIsUniqueAsync(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        int branchId,
        string? normalizedSlotCode,
        int? excludedScheduleId,
        string operation,
        CancellationToken cancellationToken)
    {
        if (normalizedSlotCode is null)
        {
            return null;
        }

        var existingId = await scheduleReadRepository.FirstOrDefaultAsync(
            new ServiceScheduleSlotCodeExistsSpec(
                branchId,
                normalizedSlotCode,
                excludedScheduleId),
            cancellationToken);

        return existingId > 0
            ? new Error(
                $"ServiceSchedules.{operation}.SlotCodeAlreadyExists",
                ServiceScheduleMessages.SlotCodeAlreadyExists,
                ErrorType.Conflict)
            : null;
    }

    public static async Task<bool> IsCurrentlyOperationalAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        BranchScheduleState branch,
        int serviceId,
        CancellationToken cancellationToken)
    {
        if (!branch.IsActive)
        {
            return false;
        }

        var context = await LoadServiceEligibilityContextAsync(
            serviceReadRepository,
            branchServiceReadRepository,
            branch.BranchId,
            cancellationToken);

        if (!context.ServicesById.TryGetValue(serviceId, out var service) ||
            !context.ServiceStates.TryGetValue(serviceId, out var state))
        {
            return false;
        }

        return
            context.AssignedServiceIds.Contains(serviceId) &&
            IsVisibleInBranch(service, branch.BranchId) &&
            !service.IsDeleted &&
            service.IsActive &&
            service.IsTicketIssuable &&
            !state.HasChildren &&
            state.EffectiveIsActive;
    }

    public static async Task<Error?> ValidateServiceCanBeViewedAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IServiceVisibilityPolicy serviceVisibilityPolicy,
        int serviceId,
        string operation,
        CancellationToken cancellationToken)
    {
        var services = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);

        var service = services.FirstOrDefault(x => x.Id == serviceId);

        if (service is null)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceNotFound",
                ServiceScheduleMessages.ServiceNotFound,
                ErrorType.NotFound);
        }

        var visibility = serviceVisibilityPolicy.EnsureCanView(
            service.Scope,
            service.OwnerBranchId,
            $"ServiceSchedules.{operation}");

        return visibility.IsFailure
            ? visibility.Errors[0]
            : null;
    }

    public static async Task<ServiceScheduleResponse?> BuildResponseAsync(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        BranchScheduleState branch,
        int serviceId,
        int? scheduleId,
        string? message,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleReadRepository.FirstOrDefaultAsync(
            new GetServiceScheduleProjectionSpec(
                branch.BranchId,
                serviceId,
                scheduleId),
            cancellationToken);

        if (schedule is null)
        {
            return null;
        }

        var isOperational = await IsCurrentlyOperationalAsync(
            serviceReadRepository,
            branchServiceReadRepository,
            branch,
            serviceId,
            cancellationToken);

        return ServiceScheduleResponseFactory.ToResponse(
            schedule,
            isOperational,
            message);
    }

    private static async Task<ServiceEligibilityContext>
        LoadServiceEligibilityContextAsync(
            IWriteReadRepository<Service> serviceReadRepository,
            IWriteReadRepository<BranchService> branchServiceReadRepository,
            int branchId,
            CancellationToken cancellationToken)
    {
        var services = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);

        var serviceStates = ServiceHierarchyCalculator.ComputeStates(services);
        var servicesById = services.ToDictionary(x => x.Id);

        var branchAssignments = await branchServiceReadRepository.ListAsync(
            new BranchServiceAssignmentsForBranchSpec(branchId),
            cancellationToken);

        return new ServiceEligibilityContext(
            servicesById,
            serviceStates,
            branchAssignments.ToHashSet());
    }

    private static Error? ValidateEligibleService(
        int serviceId,
        int branchId,
        IReadOnlyDictionary<int, ServiceHierarchyItem> servicesById,
        IReadOnlyDictionary<int, ServiceHierarchyState> serviceStates,
        IReadOnlySet<int> assignedServiceIds,
        string operation)
    {
        if (!servicesById.TryGetValue(serviceId, out var service))
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceNotFound",
                ServiceScheduleMessages.ServiceNotFound,
                ErrorType.NotFound);
        }

        if (!assignedServiceIds.Contains(service.Id) ||
            !IsVisibleInBranch(service, branchId))
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceNotAssignedToBranch",
                ServiceScheduleMessages.ServiceNotAssignedToBranch,
                service.Scope == ServiceScope.BranchScoped &&
                    service.OwnerBranchId != branchId
                    ? ErrorType.Security
                    : ErrorType.Conflict);
        }

        if (service.IsDeleted)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceDeleted",
                ServiceScheduleMessages.ServiceDeleted,
                ErrorType.Conflict);
        }

        if (!service.IsActive)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceInactive",
                ServiceScheduleMessages.ServiceInactive,
                ErrorType.Conflict);
        }

        if (!serviceStates.TryGetValue(service.Id, out var state) ||
            state.HasChildren)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceIsNotLeaf",
                ServiceScheduleMessages.ServiceIsNotLeaf,
                ErrorType.Conflict);
        }

        if (!state.EffectiveIsActive)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceNotEffectivelyActive",
                ServiceScheduleMessages.ServiceNotEffectivelyActive,
                ErrorType.Conflict);
        }

        if (!service.IsTicketIssuable)
        {
            return new Error(
                $"ServiceSchedules.{operation}.ServiceNotTicketIssuable",
                ServiceScheduleMessages.ServiceNotTicketIssuable,
                ErrorType.Conflict);
        }

        return null;
    }

    private static bool IsVisibleInBranch(
        ServiceHierarchyItem service,
        int branchId)
    {
        return service.Scope == ServiceScope.Global ||
            (service.Scope == ServiceScope.BranchScoped &&
             service.OwnerBranchId == branchId);
    }

    private sealed record ServiceEligibilityContext(
        IReadOnlyDictionary<int, ServiceHierarchyItem> ServicesById,
        IReadOnlyDictionary<int, ServiceHierarchyState> ServiceStates,
        IReadOnlySet<int> AssignedServiceIds);
}

internal sealed class BranchServiceAssignmentsForBranchSpec
    : BuildingBlock.Domain.Specification.Specification<BranchService, int>
{
    public BranchServiceAssignmentsForBranchSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x => x.BranchId == branchId);
        Select(x => x.ServiceId);
    }
}
