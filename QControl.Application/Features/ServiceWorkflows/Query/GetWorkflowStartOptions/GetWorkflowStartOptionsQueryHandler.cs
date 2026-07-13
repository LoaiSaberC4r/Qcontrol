using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;

internal sealed class GetWorkflowStartOptionsQueryHandler
    : IQueryHandler<GetWorkflowStartOptionsQuery, ServiceWorkflowStartOptionsResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetWorkflowStartOptionsQueryHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser,
        IServiceVisibilityPolicy visibilityPolicy)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _visibilityPolicy = visibilityPolicy
            ?? throw new ArgumentNullException(nameof(visibilityPolicy));
    }

    public async Task<Result<ServiceWorkflowStartOptionsResponse>> Handle(
        GetWorkflowStartOptionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var requestedService = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(request.ServiceId),
            cancellationToken);
        if (requestedService is null)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(new Error(
                "ServiceWorkflows.StartOptions.ServiceNotFound",
                ServiceWorkflowMessages.ServiceNotFound,
                ErrorType.NotFound));
        }

        var visibility = _visibilityPolicy.EnsureCanView(
            requestedService.Scope,
            requestedService.OwnerBranchId,
            "ServiceWorkflows.StartOptions");
        if (visibility.IsFailure)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(
                visibility.Errors);
        }

        var serviceError =
            await ServiceWorkflowRuleChecks.ValidateStepServicesAreEligibleAsync(
                _serviceReadRepository,
                new[]
                {
                    new ServiceWorkflowStepCommandItem
                    {
                        ServiceId = request.ServiceId,
                        StepOrder = 1
                    }
                },
                operation: "StartOptions",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(
                serviceError);
        }

        var workflows = await _workflowReadRepository.Query()
            .Where(x => x.IsActive)
            .Where(x => x.Steps.Any(step =>
                step.StepOrder == 1 &&
                step.ServiceId == request.ServiceId))
            .OrderBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
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
            .ToListAsync(cancellationToken);

        var workflowItems = await BuildWorkflowItemsAsync(
            workflows,
            request.ServiceId,
            cancellationToken);

        var mode = workflowItems.Count switch
        {
            0 => "NoWorkflow",
            1 => "AutoApply",
            _ => "SelectionRequired"
        };

        return Result<ServiceWorkflowStartOptionsResponse>.Ok(
            new ServiceWorkflowStartOptionsResponse
            {
                ServiceId = request.ServiceId,
                Mode = mode,
                RequiresWorkflowSelection = workflowItems.Count > 1,
                AutoApplyWorkflowId = workflowItems.Count == 1
                    ? workflowItems[0].WorkflowId
                    : null,
                Workflows = workflowItems
            });
    }

    private async Task<IReadOnlyList<ServiceWorkflowStartOptionItemResponse>>
        BuildWorkflowItemsAsync(
            IReadOnlyList<ServiceWorkflowBasicProjection> workflows,
            int matchedServiceId,
            CancellationToken cancellationToken)
    {
        if (workflows.Count == 0)
        {
            return Array.Empty<ServiceWorkflowStartOptionItemResponse>();
        }

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        serviceItems = serviceItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var servicesById = serviceItems.ToDictionary(x => x.Id);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);

        var workflowIds = workflows
            .Select(x => x.WorkflowId)
            .ToArray();

        var steps = await _stepReadRepository.Query()
            .Where(x => workflowIds.Contains(x.ServiceWorkflowId))
            .OrderBy(x => x.ServiceWorkflowId)
            .ThenBy(x => x.StepOrder)
            .ThenBy(x => x.ServiceId)
            .Select(x => new ServiceWorkflowStepProjection
            {
                WorkflowId = x.ServiceWorkflowId,
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToListAsync(cancellationToken);

        var stepsByWorkflowId = steps
            .GroupBy(x => x.WorkflowId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyCollection<ServiceWorkflowStepProjection>)
                    x.ToList());

        return workflows
            .Select(workflow =>
            {
                var workflowSteps = stepsByWorkflowId.TryGetValue(
                    workflow.WorkflowId,
                    out var foundSteps)
                    ? foundSteps
                    : Array.Empty<ServiceWorkflowStepProjection>();

                var stepResponses = workflowSteps
                    .OrderBy(x => x.StepOrder)
                    .ThenBy(x => x.ServiceId)
                    .Select(x => ServiceWorkflowResponseFactory.ToStepResponse(
                        x,
                        servicesById,
                        serviceStates,
                        matchedServiceId))
                    .ToList();

                return new ServiceWorkflowStartOptionItemResponse
                {
                    WorkflowId = workflow.WorkflowId,
                    ArabicName = workflow.ArabicName,
                    EnglishName = workflow.EnglishName,
                    IsActive = workflow.IsActive,
                    StartServiceId = stepResponses.FirstOrDefault()?.ServiceId,
                    StepsCount = stepResponses.Count,
                    Steps = stepResponses
                };
            })
            .OrderBy(x => x.ArabicName)
            .ThenBy(x => x.WorkflowId)
            .ToList();
    }
}
