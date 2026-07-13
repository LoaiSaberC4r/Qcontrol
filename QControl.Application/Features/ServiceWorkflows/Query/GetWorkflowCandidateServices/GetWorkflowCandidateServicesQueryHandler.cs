using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;

internal sealed class GetWorkflowCandidateServicesQueryHandler
    : IQueryHandler<GetWorkflowCandidateServicesQuery, Pagination<ServiceWorkflowCandidateServiceResponse>>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetWorkflowCandidateServicesQueryHandler(
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

    public async Task<Result<Pagination<ServiceWorkflowCandidateServiceResponse>>> Handle(
        GetWorkflowCandidateServicesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<ServiceWorkflowCandidateServiceResponse>>.Fail(
                new Error(
                    "ServiceWorkflows.Authentication.Required",
                    ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var visibilityContext = _visibilityPolicy.EnsureCanUseVisibilityContext(
            "ServiceWorkflows.CandidateServices");
        if (visibilityContext.IsFailure)
        {
            return Result<Pagination<ServiceWorkflowCandidateServiceResponse>>
                .Fail(visibilityContext.Errors);
        }

        request.SearchText ??= string.Empty;

        var query = _visibilityPolicy.ApplyVisibleServices(
                _serviceReadRepository.Query())
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive &&
                x.IsTicketIssuable);

        if (request.ParentServiceId.HasValue)
        {
            var parentServiceId = request.ParentServiceId.Value;
            query = query.Where(x => x.ParentServiceId == parentServiceId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(x =>
                x.ArabicName.Contains(searchText) ||
                x.EnglishName.Contains(searchText));
        }

        var databaseFilteredItems = await query
            .OrderBy(x => x.ParentServiceId ?? 0)
            .ThenBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Select(ServiceProjection.ToHierarchyItem)
            .ToListAsync(cancellationToken);

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        serviceItems = serviceItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);
        var servicesById = serviceItems.ToDictionary(x => x.Id);
        var eligibleItems = databaseFilteredItems
            .Where(x =>
                serviceStates.TryGetValue(x.Id, out var state) &&
                state.EffectiveIsActive)
            .ToList();
        var totalCount = eligibleItems.Count;
        var pageItems = eligibleItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var workflowsByService =
            await ServiceWorkflowReadHelpers.LoadContainingWorkflowsByServiceAsync(
                _workflowReadRepository,
                _stepReadRepository,
                pageItems.Select(x => x.Id).ToList(),
                servicesById,
                serviceStates,
                cancellationToken);

        var responses = pageItems
            .Select(item =>
            {
                ServiceHierarchyItem? parent = null;
                if (item.ParentServiceId.HasValue)
                {
                    servicesById.TryGetValue(
                        item.ParentServiceId.Value,
                        out parent);
                }

                serviceStates.TryGetValue(item.Id, out var state);
                workflowsByService.TryGetValue(
                    item.Id,
                    out var workflows);

                return new ServiceWorkflowCandidateServiceResponse
                {
                    ServiceId = item.Id,
                    ParentServiceId = item.ParentServiceId,
                    ParentArabicName = parent?.ArabicName,
                    ParentEnglishName = parent?.EnglishName,
                    ArabicName = item.ArabicName,
                    EnglishName = item.EnglishName,
                    IsActive = item.IsActive,
                    IsDeleted = item.IsDeleted,
                    EffectiveIsActive = state?.EffectiveIsActive ?? false,
                    IsTicketIssuable = item.IsTicketIssuable,
                    Workflows = workflows ??
                        Array.Empty<ServiceWorkflowContainingServiceResponse>()
                };
            })
            .ToList();

        return Result<Pagination<ServiceWorkflowCandidateServiceResponse>>.Ok(
            new Pagination<ServiceWorkflowCandidateServiceResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                responses));
    }
}
