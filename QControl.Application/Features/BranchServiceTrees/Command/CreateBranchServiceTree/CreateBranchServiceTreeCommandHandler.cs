using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceTrees.Shared;
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
    private readonly ICurrentUser _currentUser;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchServiceTreeCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteRepository<Service> serviceWriteRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        ICurrentUser currentUser,
        IServiceDefinitionAccessValidator accessValidator,
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
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
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
            });
        if (payloadValidation is not null)
        {
            return Result<CreateBranchServiceTreeResponse>.Fail(
                payloadValidation);
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
            when (ServiceUniqueConstraintErrorMapper.TryMapCreate(
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
