using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServices.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;

internal sealed class UnassignBranchLeafServiceCommandHandler
    : ICommandHandler<
        UnassignBranchLeafServiceCommand,
        UnassignBranchLeafServiceResponse>
{
    private const string CodePrefix = "BranchServices.Unassign";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;

    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;

    private readonly IWriteRepository<BranchService>
        _branchServiceWriteRepository;
    private readonly IWriteReadRepository<BranchServiceSegment>?
        _branchServiceSegmentReadRepository;
    private readonly IWriteRepository<BranchServiceSegment>?
        _branchServiceSegmentWriteRepository;

    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignBranchLeafServiceCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IUnitOfWork unitOfWork)
        : this(
            branchReadRepository,
            serviceReadRepository,
            branchServiceReadRepository,
            branchServiceWriteRepository,
            branchServiceSegmentReadRepository: null,
            branchServiceSegmentWriteRepository: null,
            currentUser,
            accessValidator,
            unitOfWork)
    {
    }

    public UnassignBranchLeafServiceCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        IWriteReadRepository<BranchServiceSegment>?
            branchServiceSegmentReadRepository,
        IWriteRepository<BranchServiceSegment>?
            branchServiceSegmentWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchReadRepository));

        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(
                nameof(serviceReadRepository));

        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceReadRepository));

        _branchServiceWriteRepository = branchServiceWriteRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceWriteRepository));
        _branchServiceSegmentReadRepository =
            branchServiceSegmentReadRepository;
        _branchServiceSegmentWriteRepository =
            branchServiceSegmentWriteRepository;

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UnassignBranchLeafServiceResponse>> Handle(
        UnassignBranchLeafServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Failure(
                $"{CodePrefix}.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var branchAccess = _accessValidator.EnsureCanAccessTargetBranch(
            request.BranchId,
            CodePrefix);

        if (branchAccess.IsFailure)
        {
            return Result<UnassignBranchLeafServiceResponse>.Fail(
                branchAccess.Errors);
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchForBranchServiceOperationSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Failure(
                $"{CodePrefix}.BranchNotFound",
                ServiceFeatureMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        if (!branch.IsActive)
        {
            return Failure(
                $"{CodePrefix}.BranchInactive",
                ServiceFeatureMessages.BranchInactive,
                ErrorType.Conflict);
        }

        var hierarchyItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);

        var branchAssignments =
            await _branchServiceReadRepository.ListAsync(
                new GetBranchServicesForUnassignmentSpec(request.BranchId),
                cancellationToken);

        var assignedServiceIds = branchAssignments
            .Select(x => x.ServiceId)
            .ToHashSet();

        var planningError =
            BranchServiceUnassignmentPlanner.TryBuildPlan(
                request.LeafServiceId,
                hierarchyItems,
                assignedServiceIds,
                out var plannedServiceIds);

        if (planningError is not null)
        {
            return Result<UnassignBranchLeafServiceResponse>.Fail(
                planningError);
        }

        var assignmentsByServiceId = branchAssignments
            .ToDictionary(x => x.ServiceId);

        var assignmentsToDelete = plannedServiceIds
            .Where(assignmentsByServiceId.ContainsKey)
            .Select(serviceId => assignmentsByServiceId[serviceId])
            .ToArray();

        if (_branchServiceSegmentReadRepository is not null &&
            _branchServiceSegmentWriteRepository is not null)
        {
            var branchServiceIds = assignmentsToDelete
                .Select(x => x.Id)
                .ToArray();
            var segmentAssignments =
                await _branchServiceSegmentReadRepository
                    .Query()
                    .AsTracking()
                    .Where(x =>
                        branchServiceIds.Contains(x.BranchServiceId))
                    .ToListAsync(cancellationToken);
            _branchServiceSegmentWriteRepository.DeleteRange(
                segmentAssignments);
        }
        _branchServiceWriteRepository.DeleteRange(
            assignmentsToDelete);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                $"{CodePrefix}.PersistenceConflict",
                ServiceFeatureMessages
                    .BranchServicesUnassignPersistenceConflict,
                ErrorType.Conflict);
        }

        var removedServiceIds = assignmentsToDelete
            .Select(x => x.ServiceId)
            .ToArray();

        return Result<UnassignBranchLeafServiceResponse>.Ok(
            new UnassignBranchLeafServiceResponse
            {
                BranchId = request.BranchId,
                LeafServiceId = request.LeafServiceId,
                UnassignedServiceIds = removedServiceIds,
                RemovedAssignmentsCount = removedServiceIds.Length,
                Message = ServiceFeatureMessages
                    .BranchServicesUnassignSuccess
            });
    }

    private static Result<UnassignBranchLeafServiceResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<UnassignBranchLeafServiceResponse>.Fail(
            new Error(
                code,
                message,
                type));
}
