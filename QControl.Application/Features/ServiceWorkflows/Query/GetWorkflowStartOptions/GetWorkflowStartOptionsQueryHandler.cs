using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;

internal sealed class GetWorkflowStartOptionsQueryHandler
    : IQueryHandler<GetWorkflowStartOptionsQuery, ServiceWorkflowStartOptionsResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetWorkflowStartOptionsQueryHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
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

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(
                request.ServiceId,
                includeDeleted: true),
            cancellationToken);

        if (service is null)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(new Error(
                "ServiceWorkflows.StartOptions.ServiceNotFound",
                ServiceWorkflowMessages.ServiceNotFound,
                ErrorType.NotFound));
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
            .Select(x => new ServiceWorkflowStartOptionItemResponse
            {
                WorkflowId = x.Id,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
                IsActive = x.IsActive,
                StepsCount = x.Steps.Count
            })
            .ToListAsync(cancellationToken);

        var mode = workflows.Count switch
        {
            0 => "NoWorkflow",
            1 => "AutoApply",
            _ => "SelectionRequired"
        };

        return Result<ServiceWorkflowStartOptionsResponse>.Ok(
            new ServiceWorkflowStartOptionsResponse
            {
                ServiceId = service.Id,
                Mode = mode,
                RequiresWorkflowSelection = workflows.Count > 1,
                AutoApplyWorkflowId = workflows.Count == 1
                    ? workflows[0].WorkflowId
                    : null,
                Workflows = workflows
            });
    }
}
