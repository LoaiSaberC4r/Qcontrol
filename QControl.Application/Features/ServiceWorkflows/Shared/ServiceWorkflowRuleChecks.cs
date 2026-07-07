using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowRuleChecks
{
    public static async Task<Error?> ValidateDuplicateNamesAsync(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        string arabicName,
        string englishName,
        int? excludedWorkflowId,
        string operation,
        CancellationToken cancellationToken)
    {
        var duplicateArabicId =
            await workflowReadRepository.FirstOrDefaultAsync(
                new ServiceWorkflowDuplicateArabicNameSpec(
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

    public static async Task<Error?> ValidateStepServicesAreEligibleAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IReadOnlyCollection<ServiceWorkflowStepCommandItem> steps,
        string operation,
        CancellationToken cancellationToken)
    {
        var requestedServiceIds = steps
            .Select(x => x.ServiceId)
            .Distinct()
            .ToHashSet();

        var services = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var servicesById = services.ToDictionary(x => x.Id);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            services);

        foreach (var serviceId in requestedServiceIds)
        {
            if (!servicesById.TryGetValue(serviceId, out var service))
            {
                return new Error(
                    $"ServiceWorkflows.{operation}.ServiceNotFound",
                    ServiceWorkflowMessages.ServiceNotFound,
                    ErrorType.NotFound);
            }

            if (service.IsDeleted)
            {
                return new Error(
                    $"ServiceWorkflows.{operation}.ServiceDeleted",
                    ServiceWorkflowMessages.ServiceDeleted,
                    ErrorType.Conflict);
            }

            if (!service.IsActive)
            {
                return new Error(
                    $"ServiceWorkflows.{operation}.ServiceInactive",
                    ServiceWorkflowMessages.ServiceInactive,
                    ErrorType.Conflict);
            }

            if (!service.IsTicketIssuable)
            {
                return new Error(
                    $"ServiceWorkflows.{operation}.ServiceNotTicketIssuable",
                    ServiceWorkflowMessages.ServiceNotTicketIssuable,
                    ErrorType.Conflict);
            }

            if (!serviceStates.TryGetValue(
                    service.Id,
                    out var state) ||
                !state.EffectiveIsActive)
            {
                return new Error(
                    $"ServiceWorkflows.{operation}.ServiceNotEffectivelyActive",
                    ServiceWorkflowMessages.ServiceNotEffectivelyActive,
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    public static Task<Error?> ValidateStepServicesAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IReadOnlyCollection<ServiceWorkflowStepCommandItem> steps,
        string operation,
        CancellationToken cancellationToken)
        => ValidateStepServicesAreEligibleAsync(
            serviceReadRepository,
            steps,
            operation,
            cancellationToken);

    public static async Task<ServiceWorkflowResponse?> BuildDetailsResponseAsync(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        int workflowId,
        string? message,
        CancellationToken cancellationToken)
    {
        var workflow = await workflowReadRepository.Query()
            .Where(x => x.Id == workflowId)
            .Select(x => new ServiceWorkflowBasicProjection
            {
                WorkflowId = x.Id,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
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
            .Where(x => x.ServiceWorkflowId == workflowId)
            .OrderBy(x => x.StepOrder)
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
}
