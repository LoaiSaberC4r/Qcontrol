using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;

internal sealed class GetServiceWorkflowsQueryHandler
    : IQueryHandler<GetServiceWorkflowsQuery, Pagination<ServiceWorkflowListItemResponse>>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServiceWorkflowsQueryHandler(
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

        request.SearchText ??= string.Empty;

        var query = _workflowReadRepository.Query();

        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            query = query.Where(x => x.IsActive == isActive);
        }

        if (request.StartServiceId.HasValue)
        {
            var startServiceId = request.StartServiceId.Value;
            query = query.Where(x => x.Steps.Any(step =>
                step.StepOrder == 1 &&
                step.ServiceId == startServiceId));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x =>
                x.ArabicName.Contains(searchText) ||
                x.EnglishName.Contains(searchText));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var workflows = await query
            .OrderBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                WorkflowId = x.Id,
                x.ArabicName,
                x.EnglishName,
                StartServiceId = x.Steps
                    .Where(step => step.StepOrder == 1)
                    .Select(step => (int?)step.ServiceId)
                    .FirstOrDefault(),
                StepsCount = x.Steps.Count,
                x.IsActive,
                x.RowVersion,
                x.CreatedOnUtc,
                x.ModifiedOnUtc
            })
            .ToListAsync(cancellationToken);

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var servicesById = serviceItems.ToDictionary(x => x.Id);

        var responseItems = workflows
            .Select(x =>
            {
                ServiceHierarchyItem? startService = null;
                if (x.StartServiceId.HasValue)
                {
                    servicesById.TryGetValue(
                        x.StartServiceId.Value,
                        out startService);
                }

                return new ServiceWorkflowListItemResponse
                {
                    WorkflowId = x.WorkflowId,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName,
                    StartServiceId = x.StartServiceId,
                    StartServiceArabicName = startService?.ArabicName,
                    StartServiceEnglishName = startService?.EnglishName,
                    StepsCount = x.StepsCount,
                    IsActive = x.IsActive,
                    RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
                    CreatedOnUtc = x.CreatedOnUtc,
                    ModifiedOnUtc = x.ModifiedOnUtc
                };
            })
            .ToList();

        return Result<Pagination<ServiceWorkflowListItemResponse>>.Ok(
            new Pagination<ServiceWorkflowListItemResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                responseItems));
    }
}
