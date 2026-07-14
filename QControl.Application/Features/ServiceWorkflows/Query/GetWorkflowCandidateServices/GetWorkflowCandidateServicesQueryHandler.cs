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
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;

internal sealed class GetWorkflowCandidateServicesQueryHandler
    : IQueryHandler<GetWorkflowCandidateServicesQuery, Pagination<ServiceWorkflowCandidateServiceResponse>>
{
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetWorkflowCandidateServicesQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
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

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "CandidateServices",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<Pagination<ServiceWorkflowCandidateServiceResponse>>
                .Fail(branchResult.Errors);
        }

        request.SearchText ??= string.Empty;

        var assignedIds = await _branchServiceReadRepository.Query()
            .Where(x => x.BranchId == request.BranchId)
            .Select(x => x.ServiceId)
            .ToArrayAsync(cancellationToken);
        var assignedSet = assignedIds.ToHashSet();

        if (assignedSet.Count == 0)
        {
            return Result<Pagination<ServiceWorkflowCandidateServiceResponse>>.Ok(
                new Pagination<ServiceWorkflowCandidateServiceResponse>(
                    request.PageNumber,
                    request.PageSize,
                    totalItems: 0,
                    data: Array.Empty<ServiceWorkflowCandidateServiceResponse>()));
        }

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);
        var servicesById = serviceItems.ToDictionary(x => x.Id);

        var eligibleItems = serviceItems
            .Where(x => assignedSet.Contains(x.Id))
            .Where(x =>
                x.Scope == ServiceScope.Global ||
                (x.Scope == ServiceScope.BranchScoped &&
                 x.OwnerBranchId == request.BranchId))
            .Where(x => !x.IsDeleted)
            .Where(x => x.IsActive)
            .Where(x => x.IsTicketIssuable)
            .Where(x =>
                serviceStates.TryGetValue(x.Id, out var state) &&
                state.EffectiveIsActive &&
                !state.HasChildren)
            .ToList();

        if (request.ParentServiceId.HasValue)
        {
            var parentServiceId = request.ParentServiceId.Value;
            eligibleItems = eligibleItems
                .Where(x => x.ParentServiceId == parentServiceId)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            eligibleItems = eligibleItems
                .Where(x =>
                    x.ArabicName.Contains(searchText) ||
                    x.EnglishName.Contains(searchText))
                .ToList();
        }

        var totalCount = eligibleItems.Count;
        var pageItems = eligibleItems
            .OrderBy(x => x.ParentServiceId ?? 0)
            .ThenBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

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

                var state = serviceStates[item.Id];

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
                    EffectiveIsActive = state.EffectiveIsActive,
                    IsTicketIssuable = item.IsTicketIssuable
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
