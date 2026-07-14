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
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetWorkflowStartOptionsQueryHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
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

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "StartOptions",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(
                branchResult.Errors);
        }

        var ownerError =
            await ServiceWorkflowRuleChecks.ValidateOwnerAndStepServicesAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                Array.Empty<ServiceWorkflowStepCommandItem>(),
                "StartOptions",
                cancellationToken);

        if (ownerError is not null)
        {
            return Result<ServiceWorkflowStartOptionsResponse>.Fail(
                ownerError);
        }

        var workflows = await _workflowReadRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.BranchId == request.BranchId &&
                x.LeafServiceId == request.LeafServiceId &&
                x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
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
            .ToListAsync(cancellationToken);

        var workflowItems = await BuildWorkflowItemsAsync(
            workflows,
            cancellationToken);
        var defaultWorkflowId = workflowItems
            .FirstOrDefault(x => x.IsDefault)
            ?.WorkflowId;

        return Result<ServiceWorkflowStartOptionsResponse>.Ok(
            new ServiceWorkflowStartOptionsResponse
            {
                ServiceId = request.LeafServiceId,
                BranchId = request.BranchId,
                LeafServiceId = request.LeafServiceId,
                HasWorkflows = workflowItems.Count > 0,
                DefaultWorkflowId = defaultWorkflowId,
                Mode = workflowItems.Count == 0
                    ? "NoWorkflow"
                    : "DefaultAvailable",
                RequiresWorkflowSelection = false,
                AutoApplyWorkflowId = defaultWorkflowId,
                Workflows = workflowItems
            });
    }

    private async Task<IReadOnlyList<ServiceWorkflowStartOptionItemResponse>>
        BuildWorkflowItemsAsync(
            IReadOnlyList<ServiceWorkflowBasicProjection> workflows,
            CancellationToken cancellationToken)
    {
        if (workflows.Count == 0)
        {
            return Array.Empty<ServiceWorkflowStartOptionItemResponse>();
        }

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var servicesById = serviceItems.ToDictionary(x => x.Id);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);

        var workflowIds = workflows
            .Select(x => x.WorkflowId)
            .ToArray();

        var steps = await _stepReadRepository.Query()
            .AsNoTracking()
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
                        matchedServiceId: null))
                    .ToList();

                return new ServiceWorkflowStartOptionItemResponse
                {
                    WorkflowId = workflow.WorkflowId,
                    ArabicName = workflow.ArabicName,
                    EnglishName = workflow.EnglishName,
                    IsActive = workflow.IsActive,
                    IsDefault = workflow.IsDefault,
                    StartServiceId = stepResponses.FirstOrDefault()?.ServiceId,
                    StepsCount = stepResponses.Count,
                    Steps = stepResponses
                };
            })
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.WorkflowId)
            .ToList();
    }
}
