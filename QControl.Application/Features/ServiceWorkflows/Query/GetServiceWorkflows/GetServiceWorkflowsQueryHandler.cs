using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;

internal sealed class GetServiceWorkflowsQueryHandler
    : IQueryHandler<GetServiceWorkflowsQuery, Pagination<ServiceWorkflowListItemResponse>>
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

    public GetServiceWorkflowsQueryHandler(
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

    public async Task<Result<Pagination<ServiceWorkflowListItemResponse>>> Handle(
        GetServiceWorkflowsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<ServiceWorkflowListItemResponse>>.Fail(
                new Error(
                    "ServiceWorkflows.Authentication.Required",
                    ServiceWorkflowMessages.AuthenticationRequired,
                    ErrorType.Unauthorized));
        }

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "ViewAll",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<Pagination<ServiceWorkflowListItemResponse>>.Fail(
                branchResult.Errors);
        }

        var ownerError =
            await ServiceWorkflowRuleChecks.ValidateOwnerAndStepServicesAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                Array.Empty<ServiceWorkflowStepCommandItem>(),
                "ViewAll",
                cancellationToken);

        if (ownerError is not null)
        {
            return Result<Pagination<ServiceWorkflowListItemResponse>>.Fail(
                ownerError);
        }

        request.SearchText ??= string.Empty;

        var query = _workflowReadRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.BranchId == request.BranchId &&
                x.LeafServiceId == request.LeafServiceId);

        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            query = query.Where(x => x.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x =>
                (x.ArabicName != null &&
                 x.ArabicName.Contains(searchText)) ||
                (x.EnglishName != null &&
                 x.EnglishName.Contains(searchText)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.OrderSort == OrderSort.Oldest
            ? query.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.CreatedOnUtc).ThenBy(x => x.Id);

        var workflows = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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

        var responseItems = await BuildListItemsAsync(
            workflows,
            cancellationToken);

        return Result<Pagination<ServiceWorkflowListItemResponse>>.Ok(
            new Pagination<ServiceWorkflowListItemResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                responseItems));
    }

    private async Task<IReadOnlyList<ServiceWorkflowListItemResponse>>
        BuildListItemsAsync(
            IReadOnlyList<ServiceWorkflowBasicProjection> workflows,
            CancellationToken cancellationToken)
    {
        if (workflows.Count == 0)
        {
            return Array.Empty<ServiceWorkflowListItemResponse>();
        }

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

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);
        var servicesById = serviceItems.ToDictionary(x => x.Id);

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

                var firstStep = stepResponses.FirstOrDefault();

                return new ServiceWorkflowListItemResponse
                {
                    WorkflowId = workflow.WorkflowId,
                    BranchId = workflow.BranchId,
                    LeafServiceId = workflow.LeafServiceId,
                    ArabicName = workflow.ArabicName,
                    EnglishName = workflow.EnglishName,
                    IsDefault = workflow.IsDefault,
                    StartServiceId = firstStep?.ServiceId,
                    StartServiceArabicName = firstStep?.ArabicName,
                    StartServiceEnglishName = firstStep?.EnglishName,
                    StepsCount = stepResponses.Count,
                    IsActive = workflow.IsActive,
                    Steps = stepResponses,
                    RowVersion = RowVersionConverter.ToBase64(
                        workflow.RowVersion),
                    CreatedOnUtc = workflow.CreatedOnUtc,
                    ModifiedOnUtc = workflow.ModifiedOnUtc
                };
            })
            .ToList();
    }
}
