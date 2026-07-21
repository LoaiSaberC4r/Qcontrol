using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;

internal sealed class GetBranchServiceTreeQueryHandler
    : IQueryHandler<GetBranchServiceTreeQuery, IReadOnlyList<ServiceTreeNodeResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService> _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IServiceDefinitionAccessValidator _accessValidator;

    public GetBranchServiceTreeQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IServiceDefinitionAccessValidator accessValidator)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
    }

    public async Task<Result<IReadOnlyList<ServiceTreeNodeResponse>>> Handle(
        GetBranchServiceTreeQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "BranchServices.View.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var branchAccess = _accessValidator.EnsureCanAccessTargetBranch(
            request.BranchId,
            "BranchServices.View");

        if (branchAccess.IsFailure)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(
                branchAccess.Errors);
        }

        if (request.IncludeDeleted &&
            !_currentBranchContext.IsSystemLevelActor)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "BranchServices.View.IncludeDeletedForbidden",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security));
        }

        var branchExists = await _branchReadRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == request.BranchId, cancellationToken);

        if (!branchExists)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Fail(new Error(
                "BranchServices.View.BranchNotFound",
                ServiceFeatureMessages.BranchNotFound,
                ErrorType.NotFound));
        }

        var assignedIds = await _branchServiceReadRepository.Query()
            .Where(x => x.BranchId == request.BranchId)
            .Select(x => x.ServiceId)
            .ToArrayAsync(cancellationToken);
        var assignedSet = assignedIds.ToHashSet();

        if (assignedSet.Count == 0)
        {
            return Result<IReadOnlyList<ServiceTreeNodeResponse>>.Ok(
                Array.Empty<ServiceTreeNodeResponse>());
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var accessContext = await BuildAccessContextAsync(cancellationToken);

        IReadOnlyDictionary<int, ServiceHierarchyItem> includedItems = allItems
            .Where(x => assignedSet.Contains(x.Id))
            .Where(x =>
                x.Scope == QControl.Domain.Enums.ServiceScope.Global ||
                (x.Scope == QControl.Domain.Enums.ServiceScope.BranchScoped &&
                 x.OwnerBranchId == request.BranchId))
            .Where(x => request.IncludeDeleted || !x.IsDeleted)
            .Where(x => request.IncludeInactive || states[x.Id].EffectiveIsActive)
            .ToDictionary(x => x.Id);

        includedItems = ServiceHierarchySearch.Apply(
            includedItems,
            request.SearchText);

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
            ServiceCode = item.ServiceCode,
            IsServiceCodeRequired = item.IsServiceCodeRequired,
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
}
