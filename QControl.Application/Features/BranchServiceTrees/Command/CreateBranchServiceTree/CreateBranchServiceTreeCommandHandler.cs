using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using Qcontrol.Application.Features.BranchServiceTrees.Shared;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;

internal sealed class CreateBranchServiceTreeCommandHandler
    : ICommandHandler<CreateBranchServiceTreeCommand, CreateBranchServiceTreeResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteRepository<Service> _serviceWriteRepository;
    private readonly IWriteRepository<BranchService> _branchServiceWriteRepository;
    private readonly IWriteRepository<ServiceGlobalizationRequest>
        _requestWriteRepository;
    private readonly IWriteReadRepository<Segment> _segmentReadRepository;
    private readonly IWriteRepository<BranchServiceSegment>
        _branchServiceSegmentWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchServiceTreeCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        IWriteRepository<ServiceGlobalizationRequest> requestWriteRepository,
        IWriteReadRepository<Segment> segmentReadRepository,
        IWriteRepository<BranchServiceSegment>
            branchServiceSegmentWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _serviceWriteRepository = serviceWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceWriteRepository));
        _branchServiceWriteRepository = branchServiceWriteRepository
            ?? throw new ArgumentNullException(nameof(branchServiceWriteRepository));
        _requestWriteRepository = requestWriteRepository
            ?? throw new ArgumentNullException(nameof(requestWriteRepository));
        _segmentReadRepository = segmentReadRepository
            ?? throw new ArgumentNullException(nameof(segmentReadRepository));
        _branchServiceSegmentWriteRepository =
            branchServiceSegmentWriteRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceSegmentWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CreateBranchServiceTreeResponse>> Handle(
        CreateBranchServiceTreeCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchServiceTrees.Create.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var createAccess = _accessValidator.EnsureCanCreateBranchScoped(
            request.BranchId,
            "BranchServiceTrees.Create");

        if (createAccess.IsFailure)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                createAccess.Errors);
        }

        var branch = await _branchReadRepository.Query()
            .IgnoreQueryFilters()
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
                "BranchServiceTrees.Create.BranchNotFound",
                ServiceFeatureMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        if (!branch.IsActive)
        {
            return Failure(
                "BranchServiceTrees.Create.BranchInactive",
                ServiceFeatureMessages.BranchInactive,
                ErrorType.Conflict);
        }

        if (request.Root is null)
        {
            return Failure(
                "BranchServiceTrees.Create.RootRequired",
                ServiceFeatureMessages.BranchServiceTreeRootRequired,
                ErrorType.Validation);
        }

        var requestedServiceCodes = new List<string>();
        var payloadValidation = BranchServiceTreePayloadValidator.Validate(
            request.Root,
            new BranchServiceTreePayloadValidationOptions
            {
                CodePrefix = "BranchServiceTrees.Create",
                MaxNodeCount = 1000,
                MaximumNodesExceededCode =
                    "BranchServiceTrees.Create.InvalidHierarchy",
                MaximumNodesExceededMessage =
                    ServiceFeatureMessages.InvalidHierarchy,
                TicketIssuableCannotHaveChildrenMessage =
                    ServiceFeatureMessages
                        .BranchServiceTreeTicketIssuableCannotHaveChildren,
                DuplicateArabicNameMessage =
                    ServiceFeatureMessages.BranchServiceTreeDuplicateArabicName,
                DuplicateEnglishNameMessage =
                    ServiceFeatureMessages.BranchServiceTreeDuplicateEnglishName
            },
            requestedServiceCodes);
        if (payloadValidation is not null)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                payloadValidation);
        }

        var duplicateServiceCode =
            await ServiceRuleChecks.ValidateServiceCodesAreUniqueAsync(
                _serviceReadRepository,
                requestedServiceCodes,
                "BranchServiceTrees.Create.ServiceCodeAlreadyExists",
                cancellationToken);

        if (duplicateServiceCode is not null)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                duplicateServiceCode);
        }

        var duplicateRoot = await ValidateDuplicateRootNamesAsync(
            request.BranchId,
            request.Root,
            cancellationToken);

        if (duplicateRoot is not null)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                duplicateRoot);
        }

        var services = new List<Service>();
        var assignments = new List<BranchService>();
        var createdRoot = BranchServiceTreeEntityBuilder.Build(
            request.Root,
            rootParentServiceId: null,
            request.BranchId,
            _currentUser.UserId.Value,
            services,
            assignments);
        var requestedOnUtc = _dateTimeProvider.UtcNow;
        var globalizationRequest = ServiceGlobalizationRequest.Create(
            request.BranchId,
            createdRoot.Service,
            ServiceGlobalizationRequestType.BranchServiceTree,
            _currentUser.UserId.Value,
            requestedOnUtc);

        foreach (var service in services)
        {
            globalizationRequest.AddService(service);
        }

        var defaultAssignments =
            await DefaultBranchServiceSegmentFactory
                .CreateForNewServicesAsync(
                    services,
                    assignments,
                    _segmentReadRepository,
                    _currentUser.UserId.Value,
                    cancellationToken);
        if (defaultAssignments.IsFailure)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                defaultAssignments.Errors);
        }

        await _serviceWriteRepository.AddRangeAsync(
            services,
            cancellationToken);
        await _branchServiceWriteRepository.AddRangeAsync(
            assignments,
            cancellationToken);
        await _requestWriteRepository.AddAsync(
            globalizationRequest,
            cancellationToken);
        if (defaultAssignments.Value.Count > 0)
        {
            await _branchServiceSegmentWriteRepository.AddRangeAsync(
                defaultAssignments.Value,
                cancellationToken);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ServiceGlobalizationRequestUniqueConstraintErrorMapper
                .TryMapCreate(ex, out var error))
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(error);
        }
        catch (DbUpdateException ex)
            when (ServiceUniqueConstraintErrorMapper
                .TryMapBranchServiceTreeCreate(
                ex,
                out var error))
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(error);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "BranchServiceTrees.Create.PersistenceConflict",
                ServiceFeatureMessages.BranchServiceTreePersistenceConflict,
                ErrorType.Conflict);
        }

        return Result<CreateBranchServiceTreeResponse>.Ok(
            new CreateBranchServiceTreeResponse
            {
                BranchId = request.BranchId,
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
                Message = ServiceFeatureMessages.BranchServiceTreeCreateSuccess
            });
    }

    private async Task<Error?> ValidateDuplicateRootNamesAsync(
        int branchId,
        CreateBranchServiceTreeNodeCommand root,
        CancellationToken cancellationToken)
    {
        var arabicName = root.ArabicName.Trim();
        var englishName = root.EnglishName.Trim();

        var duplicateArabicId = await _serviceReadRepository.FirstOrDefaultAsync(
            new ServiceDuplicateArabicNameSpec(
                arabicName,
                parentServiceId: null,
                ServiceScope.BranchScoped,
                ownerBranchId: branchId),
            cancellationToken);

        if (duplicateArabicId > 0)
        {
            return new Error(
                "BranchServiceTrees.Create.DuplicateArabicName",
                ServiceFeatureMessages.BranchServiceTreeDuplicateArabicName,
                ErrorType.Conflict);
        }

        var duplicateEnglishId = await _serviceReadRepository.FirstOrDefaultAsync(
            new ServiceDuplicateEnglishNameSpec(
                englishName,
                parentServiceId: null,
                ServiceScope.BranchScoped,
                ownerBranchId: branchId),
            cancellationToken);

        if (duplicateEnglishId > 0)
        {
            return new Error(
                "BranchServiceTrees.Create.DuplicateEnglishName",
                ServiceFeatureMessages.BranchServiceTreeDuplicateEnglishName,
                ErrorType.Conflict);
        }

        return null;
    }

    private static Result<CreateBranchServiceTreeResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<CreateBranchServiceTreeResponse>.Fail(
            new Error(code, message, type));
}
