using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using Qcontrol.Application.Features.BranchServiceTrees.Shared;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;

internal sealed class CreateBranchServiceSubtreeCommandHandler
    : ICommandHandler<CreateBranchServiceSubtreeCommand,
        CreateBranchServiceSubtreeResponse>
{
    private const string CodePrefix = "BranchServiceTrees.CreateSubtree";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IWriteRepository<BranchService>
        _branchServiceWriteRepository;
    private readonly IWriteRepository<ServiceGlobalizationRequest>
        _requestWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchServiceSubtreeCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        IWriteRepository<ServiceGlobalizationRequest> requestWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _branchServiceWriteRepository = branchServiceWriteRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceWriteRepository));
        _requestWriteRepository = requestWriteRepository
            ?? throw new ArgumentNullException(nameof(requestWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CreateBranchServiceSubtreeResponse>> Handle(
        CreateBranchServiceSubtreeCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                $"{CodePrefix}.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var createAccess = _accessValidator.EnsureCanCreateBranchScoped(
            request.BranchId,
            CodePrefix);

        if (createAccess.IsFailure)
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(
                createAccess.Errors);
        }

        var branch = await _branchReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == request.BranchId)
            .Select(x => new
            {
                x.Id,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

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

        if (request.Root is null)
        {
            return Failure(
                $"{CodePrefix}.RootRequired",
                ServiceFeatureMessages.BranchServiceSubtreeRootRequired,
                ErrorType.Validation);
        }

        var payloadValidation = BranchServiceTreePayloadValidator.Validate(
            request.Root,
            new BranchServiceTreePayloadValidationOptions
            {
                CodePrefix = CodePrefix,
                MaxNodeCount = 1000,
                MaximumNodesExceededCode =
                    $"{CodePrefix}.MaximumNodesExceeded",
                MaximumNodesExceededMessage =
                    ServiceFeatureMessages
                        .BranchServiceSubtreeMaximumNodesExceeded,
                TicketIssuableCannotHaveChildrenMessage =
                    ServiceFeatureMessages
                        .BranchServiceTreeTicketIssuableCannotHaveChildren,
                DuplicateArabicNameMessage =
                    ServiceFeatureMessages
                        .BranchServiceSubtreeDuplicateArabicName,
                DuplicateEnglishNameMessage =
                    ServiceFeatureMessages
                        .BranchServiceSubtreeDuplicateEnglishName,
                UseCodePrefixForTicketSettings = true
            });

        if (payloadValidation is not null)
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(
                payloadValidation);
        }

        var parentService = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.ParentServiceId,
                cancellationToken);

        if (parentService is null)
        {
            return Failure(
                $"{CodePrefix}.ParentNotFound",
                ServiceFeatureMessages.BranchServiceSubtreeParentNotFound,
                ErrorType.NotFound);
        }

        if (parentService.IsDeleted)
        {
            return Failure(
                $"{CodePrefix}.ParentDeleted",
                ServiceFeatureMessages.BranchServiceSubtreeParentDeleted,
                ErrorType.Conflict);
        }

        if (!parentService.IsActive)
        {
            return Failure(
                $"{CodePrefix}.ParentInactive",
                ServiceFeatureMessages.BranchServiceSubtreeParentInactive,
                ErrorType.Conflict);
        }

        if (parentService.Scope == ServiceScope.Global)
        {
            return await CreateLeafUnderGlobalParentAsync(
                request,
                parentService,
                cancellationToken);
        }

        var editAccess = _accessValidator.EnsureCanEdit(
            parentService,
            CodePrefix);

        if (editAccess.IsFailure)
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(
                editAccess.Errors);
        }

        var parentEffectiveIsActive = await IsParentEffectivelyActiveAsync(
            request.BranchId,
            request.ParentServiceId,
            cancellationToken);

        if (!parentEffectiveIsActive)
        {
            return Failure(
                $"{CodePrefix}.ParentNotEffectivelyActive",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentNotEffectivelyActive,
                ErrorType.Conflict);
        }

        var parentAssignedToBranch = await _branchServiceReadRepository.Query()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BranchId == request.BranchId &&
                    x.ServiceId == request.ParentServiceId,
                cancellationToken);

        if (!parentAssignedToBranch)
        {
            return Failure(
                $"{CodePrefix}.ParentNotAssignedToBranch",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentNotAssignedToBranch,
                ErrorType.Conflict);
        }

        if (parentService.IsTicketIssuable)
        {
            return Failure(
                $"{CodePrefix}.ParentTicketIssuable",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentTicketIssuable,
                ErrorType.Conflict);
        }

        var duplicateRoot = await ValidateDuplicateRootNamesAsync(
            request.BranchId,
            request.ParentServiceId,
            request.Root,
            cancellationToken);

        if (duplicateRoot is not null)
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(
                duplicateRoot);
        }

        var services = new List<Service>();
        var assignments = new List<BranchService>();
        var createdRoot = BranchServiceTreeEntityBuilder.Build(
            request.Root,
            rootParentServiceId: request.ParentServiceId,
            request.BranchId,
            _currentUser.UserId.Value,
            services,
            assignments);

        await _serviceWriteRepository.AddRangeAsync(
            services,
            cancellationToken);
        await _branchServiceWriteRepository.AddRangeAsync(
            assignments,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (CreateBranchServiceSubtreeUniqueConstraintErrorMapper
                .TryMap(ex, out var error))
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(error);
        }
        catch (DbUpdateException)
        {
            return Failure(
                $"{CodePrefix}.PersistenceConflict",
                ServiceFeatureMessages
                    .BranchServiceSubtreePersistenceConflict,
                ErrorType.Conflict);
        }

        return Result<CreateBranchServiceSubtreeResponse>.Ok(
            new CreateBranchServiceSubtreeResponse
            {
                BranchId = request.BranchId,
                ParentServiceId = request.ParentServiceId,
                Scope = ServiceScope.BranchScoped,
                OwnerBranchId = request.BranchId,
                CreatedServicesCount = services.Count,
                CreatedAssignmentsCount = assignments.Count,
                Root = BranchServiceTreeEntityBuilder.ToResponse(createdRoot),
                Message =
                    ServiceFeatureMessages.BranchServiceSubtreeCreateSuccess
            });
    }

    private async Task<Result<CreateBranchServiceSubtreeResponse>>
        CreateLeafUnderGlobalParentAsync(
            CreateBranchServiceSubtreeCommand request,
            Service parentService,
            CancellationToken cancellationToken)
    {
        var parentEffectiveIsActive = await IsServiceEffectivelyActiveAsync(
            request.ParentServiceId,
            cancellationToken);

        if (!parentEffectiveIsActive)
        {
            return Failure(
                $"{CodePrefix}.ParentNotEffectivelyActive",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentNotEffectivelyActive,
                ErrorType.Conflict);
        }

        var parentAssignedToBranch = await _branchServiceReadRepository.Query()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BranchId == request.BranchId &&
                    x.ServiceId == request.ParentServiceId,
                cancellationToken);

        if (!parentAssignedToBranch)
        {
            return Failure(
                $"{CodePrefix}.ParentNotAssignedToBranch",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentNotAssignedToBranch,
                ErrorType.Conflict);
        }

        if (parentService.IsTicketIssuable)
        {
            return Failure(
                $"{CodePrefix}.ParentTicketIssuable",
                ServiceFeatureMessages
                    .BranchServiceSubtreeParentTicketIssuable,
                ErrorType.Conflict);
        }

        if (request.Root!.Children.Count > 0)
        {
            return Failure(
                "BranchServiceTrees.Create.GlobalParentLeafRequired",
                ServiceGlobalizationRequestMessages
                    .LeafUnderGlobalParentCannotContainChildren,
                ErrorType.Validation);
        }

        var duplicateRoot = await ValidateDuplicateRootNamesAsync(
            request.BranchId,
            request.ParentServiceId,
            request.Root,
            cancellationToken);

        if (duplicateRoot is not null)
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(
                duplicateRoot);
        }

        var services = new List<Service>();
        var assignments = new List<BranchService>();
        var createdRoot = BranchServiceTreeEntityBuilder.Build(
            request.Root,
            rootParentServiceId: request.ParentServiceId,
            request.BranchId,
            _currentUser.UserId!.Value,
            services,
            assignments);

        var requestedOnUtc = _dateTimeProvider.UtcNow;
        var globalizationRequest = ServiceGlobalizationRequest.Create(
            request.BranchId,
            createdRoot.Service,
            ServiceGlobalizationRequestType.LeafUnderGlobalParent,
            _currentUser.UserId.Value,
            requestedOnUtc);
        globalizationRequest.AddService(createdRoot.Service);

        await _serviceWriteRepository.AddRangeAsync(
            services,
            cancellationToken);
        await _branchServiceWriteRepository.AddRangeAsync(
            assignments,
            cancellationToken);
        await _requestWriteRepository.AddAsync(
            globalizationRequest,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ServiceGlobalizationRequestUniqueConstraintErrorMapper
                .TryMapCreate(ex, out var error))
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(error);
        }
        catch (DbUpdateException ex)
            when (CreateBranchServiceSubtreeUniqueConstraintErrorMapper
                .TryMap(ex, out var error))
        {
            return Result<CreateBranchServiceSubtreeResponse>.Fail(error);
        }
        catch (DbUpdateException)
        {
            return Failure(
                $"{CodePrefix}.PersistenceConflict",
                ServiceFeatureMessages
                    .BranchServiceSubtreePersistenceConflict,
                ErrorType.Conflict);
        }

        return Result<CreateBranchServiceSubtreeResponse>.Ok(
            new CreateBranchServiceSubtreeResponse
            {
                BranchId = request.BranchId,
                ParentServiceId = request.ParentServiceId,
                Scope = ServiceScope.BranchScoped,
                OwnerBranchId = request.BranchId,
                CreatedServicesCount = services.Count,
                CreatedAssignmentsCount = assignments.Count,
                Root = BranchServiceTreeEntityBuilder.ToResponse(createdRoot),
                GlobalizationRequest =
                    new ServiceGlobalizationRequestSummaryResponse
                    {
                        RequestId = globalizationRequest.Id,
                        RequestType = globalizationRequest.RequestType,
                        Status = globalizationRequest.Status,
                        RequestedOnUtc =
                            globalizationRequest.RequestedOnUtc
                    },
                Message =
                    ServiceFeatureMessages.BranchServiceSubtreeCreateSuccess
            });
    }

    private async Task<Error?> ValidateDuplicateRootNamesAsync(
        int branchId,
        int parentServiceId,
        CreateBranchServiceTreeNodeCommand root,
        CancellationToken cancellationToken)
    {
        var arabicName = root.ArabicName.Trim();
        var englishName = root.EnglishName.Trim();

        var duplicateArabicId = await _serviceReadRepository
            .FirstOrDefaultAsync(
                new ServiceDuplicateArabicNameSpec(
                    arabicName,
                    parentServiceId,
                    ServiceScope.BranchScoped,
                    ownerBranchId: branchId),
                cancellationToken);

        if (duplicateArabicId > 0)
        {
            return new Error(
                $"{CodePrefix}.DuplicateArabicName",
                ServiceFeatureMessages
                    .BranchServiceSubtreeDuplicateArabicName,
                ErrorType.Conflict);
        }

        var duplicateEnglishId = await _serviceReadRepository
            .FirstOrDefaultAsync(
                new ServiceDuplicateEnglishNameSpec(
                    englishName,
                    parentServiceId,
                    ServiceScope.BranchScoped,
                    ownerBranchId: branchId),
                cancellationToken);

        if (duplicateEnglishId > 0)
        {
            return new Error(
                $"{CodePrefix}.DuplicateEnglishName",
                ServiceFeatureMessages
                    .BranchServiceSubtreeDuplicateEnglishName,
                ErrorType.Conflict);
        }

        return null;
    }

    private async Task<bool> IsParentEffectivelyActiveAsync(
        int branchId,
        int parentServiceId,
        CancellationToken cancellationToken)
    {
        var nodes = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x =>
                x.Scope == ServiceScope.Global ||
                (x.Scope == ServiceScope.BranchScoped &&
                 x.OwnerBranchId == branchId))
            .Select(x => new ParentHierarchyNode
            {
                Id = x.Id,
                ParentServiceId = x.ParentServiceId,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted
            })
            .ToArrayAsync(cancellationToken);

        var byId = nodes.ToDictionary(x => x.Id);

        return byId.TryGetValue(parentServiceId, out var parent) &&
            IsEffectivelyActive(
                parent,
                byId,
                new HashSet<int>());
    }

    private async Task<bool> IsServiceEffectivelyActiveAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var nodes = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => new ParentHierarchyNode
            {
                Id = x.Id,
                ParentServiceId = x.ParentServiceId,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted
            })
            .ToArrayAsync(cancellationToken);

        var byId = nodes.ToDictionary(x => x.Id);

        return byId.TryGetValue(serviceId, out var service) &&
            IsEffectivelyActive(
                service,
                byId,
                new HashSet<int>());
    }

    private static bool IsEffectivelyActive(
        ParentHierarchyNode node,
        IReadOnlyDictionary<int, ParentHierarchyNode> byId,
        ISet<int> visiting)
    {
        if (!visiting.Add(node.Id))
        {
            return false;
        }

        var isActive = !node.IsDeleted && node.IsActive;

        if (isActive && node.ParentServiceId.HasValue)
        {
            isActive = byId.TryGetValue(
                    node.ParentServiceId.Value,
                    out var parent) &&
                IsEffectivelyActive(parent, byId, visiting);
        }

        visiting.Remove(node.Id);
        return isActive;
    }

    private static Result<CreateBranchServiceSubtreeResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<CreateBranchServiceSubtreeResponse>.Fail(
            new Error(code, message, type));

    private sealed class ParentHierarchyNode
    {
        public int Id { get; init; }

        public int? ParentServiceId { get; init; }

        public bool IsActive { get; init; }

        public bool IsDeleted { get; init; }
    }
}
