using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServices.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;

internal sealed class AssignBranchServicesCommandHandler
    : ICommandHandler<AssignBranchServicesCommand, AssignBranchServicesResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService> _branchServiceReadRepository;
    private readonly IWriteRepository<BranchService> _branchServiceWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public AssignBranchServicesCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _branchServiceWriteRepository = branchServiceWriteRepository
            ?? throw new ArgumentNullException(nameof(branchServiceWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<AssignBranchServicesResponse>> Handle(
        AssignBranchServicesCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchServices.Assign.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var branchAccess = _accessValidator.EnsureCanAccessTargetBranch(
            request.BranchId,
            "BranchServices.Assign");

        if (branchAccess.IsFailure)
        {
            return Result<AssignBranchServicesResponse>.Fail(
                branchAccess.Errors);
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchForBranchServiceOperationSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Failure(
                "BranchServices.Assign.BranchNotFound",
                ServiceFeatureMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        if (!branch.IsActive)
        {
            return Failure(
                "BranchServices.Assign.BranchInactive",
                ServiceFeatureMessages.BranchInactive,
                ErrorType.Conflict);
        }

        var requestedLeafIds = request.ServiceIds
            .Distinct()
            .ToArray();

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var itemsById = allItems.ToDictionary(x => x.Id);
        var serviceIdsToAssign = new HashSet<int>();

        foreach (var leafId in requestedLeafIds)
        {
            var validation = ResolveLeafPath(
                leafId,
                itemsById,
                states,
                serviceIdsToAssign);

            if (validation is not null)
            {
                return Result<AssignBranchServicesResponse>.Fail(validation);
            }
        }

        var resolvedIds = serviceIdsToAssign
            .OrderBy(x => x)
            .ToArray();

        var existingAssignments = await _branchServiceReadRepository.ListAsync(
            new GetBranchServiceIdsSpec(request.BranchId, resolvedIds),
            cancellationToken);
        var existingSet = existingAssignments.ToHashSet();

        var newAssignments = resolvedIds
            .Where(x => !existingSet.Contains(x))
            .Select(x => BranchService.Create(
                request.BranchId,
                x,
                _currentUser.UserId.Value))
            .ToList();

        if (newAssignments.Count > 0)
        {
            await _branchServiceWriteRepository.AddRangeAsync(
                newAssignments,
                cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
                when (BranchServiceUniqueConstraintErrorMapper.TryMap(
                    ex,
                    out _))
            {
                return await BuildRaceSafeResponseAsync(
                    request.BranchId,
                    requestedLeafIds,
                    resolvedIds,
                    cancellationToken);
            }
        }

        return Result<AssignBranchServicesResponse>.Ok(
            new AssignBranchServicesResponse
            {
                BranchId = request.BranchId,
                RequestedLeafServiceIds = requestedLeafIds,
                AssignedServiceIds = resolvedIds,
                CreatedAssignmentsCount = newAssignments.Count,
                ExistingAssignmentsCount = existingSet.Count,
                Message = ServiceFeatureMessages.BranchServicesAssignSuccess
            });
    }

    private static Error? ResolveLeafPath(
        int leafId,
        IReadOnlyDictionary<int, ServiceHierarchyItem> itemsById,
        IReadOnlyDictionary<int, ServiceHierarchyState> states,
        ISet<int> serviceIdsToAssign)
    {
        if (!itemsById.TryGetValue(leafId, out var leaf))
        {
            return new Error(
                "BranchServices.Assign.ServiceNotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        if (leaf.IsDeleted)
        {
            return new Error(
                "BranchServices.Assign.ServiceDeleted",
                ServiceFeatureMessages.Deleted,
                ErrorType.Conflict);
        }

        if (!leaf.IsActive)
        {
            return new Error(
                "BranchServices.Assign.ServiceInactive",
                ServiceFeatureMessages.ServiceInactive,
                ErrorType.Conflict);
        }

        if (!states[leaf.Id].EffectiveIsActive)
        {
            return new Error(
                "BranchServices.Assign.ServiceEffectivelyInactive",
                ServiceFeatureMessages.ServiceEffectivelyInactive,
                ErrorType.Conflict);
        }

        if (states[leaf.Id].HasChildren)
        {
            return new Error(
                "BranchServices.Assign.ServiceNotLeaf",
                ServiceFeatureMessages.ServiceNotLeaf,
                ErrorType.Conflict);
        }

        if (leaf.Scope == ServiceScope.BranchScoped)
        {
            return new Error(
                "BranchServices.Assign.BranchScopedServiceNotSupported",
                ServiceFeatureMessages
                    .BranchServicesAssignBranchScopedNotSupported,
                ErrorType.Conflict);
        }

        var current = leaf;
        var visited = new HashSet<int>();

        while (true)
        {
            if (!visited.Add(current.Id))
            {
                return new Error(
                    "BranchServices.Assign.InvalidHierarchy",
                    ServiceFeatureMessages.InvalidHierarchy,
                    ErrorType.Conflict);
            }

            if (current.Scope != ServiceScope.Global ||
                current.OwnerBranchId.HasValue)
            {
                return new Error(
                    "BranchServices.Assign.InvalidScopeHierarchy",
                    ServiceFeatureMessages
                        .BranchServicesAssignInvalidScopeHierarchy,
                    ErrorType.Conflict);
            }

            serviceIdsToAssign.Add(current.Id);

            if (!current.ParentServiceId.HasValue)
            {
                return null;
            }

            if (!itemsById.TryGetValue(
                    current.ParentServiceId.Value,
                    out var parent))
            {
                return new Error(
                    "BranchServices.Assign.InvalidHierarchy",
                    ServiceFeatureMessages.InvalidHierarchy,
                    ErrorType.Conflict);
            }

            current = parent;
        }
    }

    private async Task<Result<AssignBranchServicesResponse>>
        BuildRaceSafeResponseAsync(
            int branchId,
            IReadOnlyList<int> requestedLeafIds,
            IReadOnlyList<int> resolvedIds,
            CancellationToken cancellationToken)
    {
        var assignedAfterRace = await _branchServiceReadRepository.ListAsync(
            new GetBranchServiceIdsSpec(branchId, resolvedIds),
            cancellationToken);

        if (assignedAfterRace.Count != resolvedIds.Count)
        {
            return Failure(
                "BranchServices.Assign.PersistenceConflict",
                ServiceFeatureMessages.BranchServiceTreePersistenceConflict,
                ErrorType.Conflict);
        }

        return Result<AssignBranchServicesResponse>.Ok(
            new AssignBranchServicesResponse
            {
                BranchId = branchId,
                RequestedLeafServiceIds = requestedLeafIds,
                AssignedServiceIds = resolvedIds,
                CreatedAssignmentsCount = 0,
                ExistingAssignmentsCount = assignedAfterRace.Count,
                Message = ServiceFeatureMessages.BranchServicesAssignSuccess
            });
    }

    private static Result<AssignBranchServicesResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<AssignBranchServicesResponse>.Fail(
            new Error(code, message, type));
}
