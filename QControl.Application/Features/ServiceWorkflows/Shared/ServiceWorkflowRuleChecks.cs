using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowRuleChecks
{
    public static async Task<Result> ValidateBranchAccessAndStateAsync(
        IWriteReadRepository<Branch> branchReadRepository,
        IBranchAccessValidator branchAccessValidator,
        int branchId,
        string operation,
        CancellationToken cancellationToken)
    {
        var access = branchAccessValidator.EnsureCanAccessBranch(
            branchId,
            $"ServiceWorkflows.{operation}");

        if (access.IsFailure)
        {
            return access;
        }

        var branch = await branchReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == branchId)
            .Select(x => new
            {
                x.Id,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null)
        {
            return Result.Fail(new Error(
                $"ServiceWorkflows.{operation}.BranchNotFound",
                ServiceWorkflowMessages.BranchNotFound,
                ErrorType.NotFound));
        }

        if (!branch.IsActive)
        {
            return Result.Fail(new Error(
                $"ServiceWorkflows.{operation}.BranchInactive",
                ServiceWorkflowMessages.BranchInactive,
                ErrorType.Conflict));
        }

        return Result.Ok();
    }

    public static async Task<Error?> ValidateDuplicateNamesAsync(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        int branchId,
        int leafServiceId,
        string arabicName,
        string englishName,
        int? excludedWorkflowId,
        string operation,
        CancellationToken cancellationToken)
    {
        var duplicateArabicId =
            await workflowReadRepository.FirstOrDefaultAsync(
                new ServiceWorkflowDuplicateArabicNameSpec(
                    branchId,
                    leafServiceId,
                    arabicName,
                    excludedWorkflowId),
                cancellationToken);

        if (duplicateArabicId > 0)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.DuplicateArabicName",
                ServiceWorkflowMessages.NameAlreadyExists,
                ErrorType.Conflict);
        }

        var duplicateEnglishId =
            await workflowReadRepository.FirstOrDefaultAsync(
                new ServiceWorkflowDuplicateEnglishNameSpec(
                    branchId,
                    leafServiceId,
                    englishName,
                    excludedWorkflowId),
                cancellationToken);

        if (duplicateEnglishId > 0)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.DuplicateEnglishName",
                ServiceWorkflowMessages.NameAlreadyExists,
                ErrorType.Conflict);
        }

        return null;
    }

    public static async Task<Error?> ValidateOwnerAndStepServicesAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        int branchId,
        int leafServiceId,
        IReadOnlyCollection<ServiceWorkflowStepCommandItem> steps,
        string operation,
        CancellationToken cancellationToken)
    {
        var services = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var servicesById = services.ToDictionary(x => x.Id);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            services);

        var assignedServiceIds = await branchServiceReadRepository.Query()
            .Where(x => x.BranchId == branchId)
            .Select(x => x.ServiceId)
            .ToArrayAsync(cancellationToken);
        var assignedSet = assignedServiceIds.ToHashSet();

        var ownerError = ValidateEligibleService(
            leafServiceId,
            branchId,
            servicesById,
            serviceStates,
            assignedSet,
            operation,
            isOwner: true);

        if (ownerError is not null)
        {
            return ownerError;
        }

        foreach (var serviceId in steps.Select(x => x.ServiceId).Distinct())
        {
            var stepError = ValidateEligibleService(
                serviceId,
                branchId,
                servicesById,
                serviceStates,
                assignedSet,
                operation,
                isOwner: false);

            if (stepError is not null)
            {
                return stepError;
            }
        }

        return null;
    }

    public static async Task<ServiceWorkflowResponse?> BuildDetailsResponseAsync(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        int branchId,
        int leafServiceId,
        int workflowId,
        string? message,
        CancellationToken cancellationToken)
    {
        var workflow = await workflowReadRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.Id == workflowId &&
                x.BranchId == branchId &&
                x.LeafServiceId == leafServiceId)
            .Select(x => new ServiceWorkflowBasicProjection
            {
                WorkflowId = x.Id,
                BranchId = x.BranchId,
                LeafServiceId = x.LeafServiceId,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                RowVersion = x.RowVersion,
                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                LastModifiedByApplicationUserId =
                    x.LastModifiedByApplicationUserId,
                DeactivatedByApplicationUserId =
                    x.DeactivatedByApplicationUserId,
                DeactivatedOnUtc = x.DeactivatedOnUtc,
                ReactivatedByApplicationUserId =
                    x.ReactivatedByApplicationUserId,
                ReactivatedOnUtc = x.ReactivatedOnUtc,
                CreatedOnUtc = x.CreatedOnUtc,
                ModifiedOnUtc = x.ModifiedOnUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workflow is null)
        {
            return null;
        }

        var steps = await stepReadRepository.Query()
            .AsNoTracking()
            .Where(x => x.ServiceWorkflowId == workflowId)
            .OrderBy(x => x.StepOrder)
            .ThenBy(x => x.ServiceId)
            .Select(x => new ServiceWorkflowStepProjection
            {
                WorkflowId = x.ServiceWorkflowId,
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToListAsync(cancellationToken);

        var serviceItems = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);
        var servicesById = serviceItems.ToDictionary(x => x.Id);

        return ServiceWorkflowResponseFactory.ToDetails(
            workflow,
            steps,
            servicesById,
            serviceStates,
            message);
    }

    public static IReadOnlyList<ServiceWorkflowStepCommandItem>
        NormalizeSteps(IEnumerable<ServiceWorkflowStepCommandItem> steps)
    {
        return steps
            .OrderBy(x => x.StepOrder)
            .Select(x => new ServiceWorkflowStepCommandItem
            {
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToList();
    }

    public static IReadOnlyList<ServiceWorkflowStepData> ToDomainSteps(
        IEnumerable<ServiceWorkflowStepCommandItem> steps)
    {
        return steps
            .OrderBy(x => x.StepOrder)
            .Select(x => new ServiceWorkflowStepData(
                x.ServiceId,
                x.StepOrder))
            .ToList();
    }

    private static Error? ValidateEligibleService(
        int serviceId,
        int branchId,
        IReadOnlyDictionary<int, ServiceHierarchyItem> servicesById,
        IReadOnlyDictionary<int, ServiceHierarchyState> serviceStates,
        IReadOnlySet<int> assignedServiceIds,
        string operation,
        bool isOwner)
    {
        var subject = isOwner ? "LeafService" : "WorkflowStep";
        var notFoundMessage = isOwner
            ? ServiceWorkflowMessages.LeafServiceNotFound
            : ServiceWorkflowMessages.ServiceNotFound;

        if (!servicesById.TryGetValue(serviceId, out var service))
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}NotFound",
                notFoundMessage,
                ErrorType.NotFound);
        }

        if (!assignedServiceIds.Contains(service.Id) ||
            (service.Scope == ServiceScope.BranchScoped &&
             service.OwnerBranchId != branchId))
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}NotAssignedToBranch",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceNotAssignedToBranch
                    : ServiceWorkflowMessages.WorkflowStepNotAssignedToBranch,
                service.Scope == ServiceScope.BranchScoped &&
                    service.OwnerBranchId != branchId
                    ? ErrorType.Security
                    : ErrorType.Conflict);
        }

        if (service.IsDeleted)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}Deleted",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceDeleted
                    : ServiceWorkflowMessages.ServiceDeleted,
                ErrorType.Conflict);
        }

        if (!service.IsActive)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}Inactive",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceInactive
                    : ServiceWorkflowMessages.ServiceInactive,
                ErrorType.Conflict);
        }

        if (!serviceStates.TryGetValue(service.Id, out var state) ||
            state.HasChildren)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}IsNotLeaf",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceIsNotLeaf
                    : ServiceWorkflowMessages.WorkflowStepIsNotLeaf,
                ErrorType.Conflict);
        }

        if (!state.EffectiveIsActive)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}NotEffectivelyActive",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceNotEffectivelyActive
                    : ServiceWorkflowMessages.ServiceNotEffectivelyActive,
                ErrorType.Conflict);
        }

        if (!service.IsTicketIssuable)
        {
            return new Error(
                $"ServiceWorkflows.{operation}.{subject}NotTicketIssuable",
                isOwner
                    ? ServiceWorkflowMessages.LeafServiceNotTicketIssuable
                    : ServiceWorkflowMessages.ServiceNotTicketIssuable,
                ErrorType.Conflict);
        }

        return null;
    }
}
