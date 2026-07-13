using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Query.GetAvailableParentServices;

internal sealed class GetAvailableParentServicesQueryHandler
    : IQueryHandler<GetAvailableParentServicesQuery, IReadOnlyList<AvailableParentServiceResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetAvailableParentServicesQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceVisibilityPolicy visibilityPolicy)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceReadRepository));
        _ticketUsageChecker = ticketUsageChecker
            ?? throw new ArgumentNullException(nameof(ticketUsageChecker));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _visibilityPolicy = visibilityPolicy
            ?? throw new ArgumentNullException(nameof(visibilityPolicy));
    }

    public async Task<Result<IReadOnlyList<AvailableParentServiceResponse>>> Handle(
        GetAvailableParentServicesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<AvailableParentServiceResponse>>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var visibilityContext = _visibilityPolicy.EnsureCanUseVisibilityContext(
            "Services.AvailableParents");
        if (visibilityContext.IsFailure)
        {
            return Result<IReadOnlyList<AvailableParentServiceResponse>>.Fail(
                visibilityContext.Errors);
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        allItems = allItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var excludedDescendants = request.ExcludeServiceId.HasValue
            ? ServiceHierarchyCalculator.GetDescendantIds(
                allItems,
                request.ExcludeServiceId.Value)
            : new HashSet<int>();
        var editedItem = request.ExcludeServiceId.HasValue
            ? allItems.FirstOrDefault(x => x.Id == request.ExcludeServiceId.Value)
            : null;
        var assignedGlobalParentIds = await LoadAssignedGlobalParentIdsAsync(
            cancellationToken);

        var candidates = allItems
            .Where(x => !x.IsDeleted)
            .Where(x => x.IsActive)
            .Where(x => !x.IsTicketIssuable)
            .Where(x =>
                !request.ExcludeServiceId.HasValue ||
                x.Id != request.ExcludeServiceId.Value)
            .Where(x => !excludedDescendants.Contains(x.Id));

        if (_currentBranchContext.IsBranchActor &&
            _currentBranchContext.ActiveBranchId.HasValue)
        {
            var branchId = _currentBranchContext.ActiveBranchId.Value;
            candidates = candidates.Where(x =>
                (x.Scope == ServiceScope.Global &&
                 assignedGlobalParentIds.Contains(x.Id)) ||
                (x.Scope == ServiceScope.BranchScoped &&
                 x.OwnerBranchId == branchId));
        }
        else
        {
            var (requiredScope, requiredOwnerBranchId) =
                ResolveRequiredScopeAndOwner(editedItem);
            candidates = candidates
                .Where(x => x.Scope == requiredScope)
                .Where(x => x.OwnerBranchId == requiredOwnerBranchId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            candidates = candidates.Where(x =>
                x.ArabicName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.EnglishName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        var responses = new List<AvailableParentServiceResponse>();

        foreach (var candidate in candidates
                     .OrderBy(x => x.ParentServiceId ?? 0)
                     .ThenBy(x => x.OrderNo)
                     .ThenBy(x => x.ArabicName)
                     .ThenBy(x => x.Id))
        {
            var hasHistoricalTickets =
                await _ticketUsageChecker.HasHistoricalTicketsAsync(
                    candidate.Id,
                    cancellationToken);

            if (hasHistoricalTickets)
            {
                continue;
            }

            responses.Add(new AvailableParentServiceResponse
            {
                Id = candidate.Id,
                ParentServiceId = candidate.ParentServiceId,
                Scope = candidate.Scope,
                OwnerBranchId = candidate.OwnerBranchId,
                OwnerBranchArabicName = candidate.OwnerBranchArabicName,
                OwnerBranchEnglishName = candidate.OwnerBranchEnglishName,
                ArabicName = candidate.ArabicName,
                EnglishName = candidate.EnglishName,
                EffectiveIsActive = states[candidate.Id].EffectiveIsActive
            });
        }

        return Result<IReadOnlyList<AvailableParentServiceResponse>>.Ok(responses);
    }

    private async Task<IReadOnlySet<int>> LoadAssignedGlobalParentIdsAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentBranchContext.IsBranchActor ||
            !_currentBranchContext.ActiveBranchId.HasValue)
        {
            return new HashSet<int>();
        }

        var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
        var assignedIds = await _branchServiceReadRepository.Query()
            .Where(x => x.BranchId == activeBranchId)
            .Select(x => x.ServiceId)
            .ToArrayAsync(cancellationToken);

        return assignedIds.ToHashSet();
    }

    private (ServiceScope Scope, int? OwnerBranchId) ResolveRequiredScopeAndOwner(
        ServiceHierarchyItem? editedItem)
    {
        if (editedItem is not null)
        {
            if (_currentBranchContext.IsBranchActor &&
                editedItem.Scope == ServiceScope.BranchScoped &&
                editedItem.OwnerBranchId == _currentBranchContext.ActiveBranchId)
            {
                return (ServiceScope.BranchScoped, editedItem.OwnerBranchId);
            }

            if (_currentBranchContext.IsSystemLevelActor)
            {
                return (editedItem.Scope, editedItem.OwnerBranchId);
            }
        }

        if (_currentBranchContext.IsBranchActor &&
            _currentBranchContext.ActiveBranchId.HasValue)
        {
            return (
                ServiceScope.BranchScoped,
                _currentBranchContext.ActiveBranchId.Value);
        }

        return (ServiceScope.Global, null);
    }
}
