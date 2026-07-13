using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServicesTree;

internal sealed class GetServicesTreeQueryHandler
    : IQueryHandler<GetServicesTreeQuery, IReadOnlyList<ServiceTreeNodeResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService> _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetServicesTreeQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceVisibilityPolicy visibilityPolicy)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _visibilityPolicy = visibilityPolicy
            ?? throw new ArgumentNullException(nameof(visibilityPolicy));
    }

    public async Task<Result<IReadOnlyList<ServiceTreeNodeResponse>>> Handle(
        GetServicesTreeQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (request.IncludeDeleted &&
            !_currentBranchContext.IsSystemLevelActor)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "Services.Tree.IncludeDeletedForbidden",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security));
        }

        var visibilityContext = _visibilityPolicy.EnsureCanUseVisibilityContext(
            "Services.Tree");
        if (visibilityContext.IsFailure)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(
                visibilityContext.Errors);
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        allItems = allItems
            .Where(x => _visibilityPolicy.CanView(x.Scope, x.OwnerBranchId))
            .ToList();
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var accessContext = await BuildAccessContextAsync(cancellationToken);

        var includedItems = allItems
            .Where(x => request.IncludeDeleted || !x.IsDeleted)
            .Where(x => request.IncludeInactive || states[x.Id].EffectiveIsActive)
            .Where(x => !request.Scope.HasValue || x.Scope == request.Scope.Value)
            .Where(x =>
                !request.OwnerBranchId.HasValue ||
                x.OwnerBranchId == request.OwnerBranchId.Value)
            .ToDictionary(x => x.Id);

        var roots = includedItems.Values
            .Where(x =>
                !x.ParentServiceId.HasValue ||
                !includedItems.ContainsKey(x.ParentServiceId.Value))
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Select(x => BuildNode(x, includedItems, states, accessContext))
            .ToList();

        return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Ok(roots);
    }

    private static ServiceTreeNodeResponse BuildNode(
        ServiceHierarchyItem item,
        IReadOnlyDictionary<int, ServiceHierarchyItem> includedItems,
        IReadOnlyDictionary<int, ServiceHierarchyState> states,
        ServiceResponseAccessContext accessContext)
    {
        var state = states[item.Id];
        var access = accessContext.ForService(item);
        var children = includedItems.Values
            .Where(x => x.ParentServiceId == item.Id)
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.Id)
            .Select(x => BuildNode(x, includedItems, states, accessContext))
            .ToList();

        return new ServiceTreeNodeResponse
        {
            Id = item.Id,
            ParentServiceId = item.ParentServiceId,
            Scope = item.Scope,
            OwnerBranchId = item.OwnerBranchId,
            OwnerBranchArabicName = item.OwnerBranchArabicName,
            OwnerBranchEnglishName = item.OwnerBranchEnglishName,
            IsOwnedByCurrentBranch = access.IsOwnedByCurrentBranch,
            IsAssignedToCurrentBranch = access.IsAssignedToCurrentBranch,
            CanEditDefinition = access.CanEditDefinition,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            IsActive = item.IsActive,
            EffectiveIsActive = state.EffectiveIsActive,
            IsTicketIssuable = item.IsTicketIssuable,
            HasChildren = state.HasChildren,
            CanIssueTicket = state.CanIssueTicket,
            IsDeleted = item.IsDeleted,
            OrderNo = item.OrderNo,
            Children = children
        };
    }

    private async Task<ServiceResponseAccessContext> BuildAccessContextAsync(
        CancellationToken cancellationToken)
    {
        var assignedServiceIds = Array.Empty<int>();

        if (_currentBranchContext.ActiveBranchId.HasValue)
        {
            var activeBranchId = _currentBranchContext.ActiveBranchId.Value;
            assignedServiceIds = await _branchServiceReadRepository.Query()
                .Where(x => x.BranchId == activeBranchId)
                .Select(x => x.ServiceId)
                .ToArrayAsync(cancellationToken);
        }

        return new ServiceResponseAccessContext(
            _currentBranchContext.IsSystemLevelActor,
            _currentBranchContext.IsBranchActor,
            _currentBranchContext.ActiveBranchId,
            assignedServiceIds);
    }
}
