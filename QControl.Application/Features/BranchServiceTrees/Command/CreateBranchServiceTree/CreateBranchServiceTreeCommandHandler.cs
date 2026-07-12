using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;

internal sealed class CreateBranchServiceTreeCommandHandler
    : ICommandHandler<CreateBranchServiceTreeCommand, CreateBranchServiceTreeResponse>
{
    private const int MaxNodeCount = 1000;

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

        var payloadValidation = ValidateTreePayload(request.Root);
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
        var createdRoot = BuildEntities(
            request.Root,
            parentService: null,
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
                Root = ToResponse(createdRoot),
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

    private static Error? ValidateTreePayload(
        CreateBranchServiceTreeNodeCommand root)
    {
        var total = 0;
        var stack = new Stack<CreateBranchServiceTreeNodeCommand>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            total++;

            if (total > MaxNodeCount)
            {
                return new Error(
                    "BranchServiceTrees.Create.InvalidHierarchy",
                    ServiceFeatureMessages.InvalidHierarchy,
                    ErrorType.Validation);
            }

            var nodeValidation = ValidateNodeFields(node);
            if (nodeValidation is not null)
            {
                return nodeValidation;
            }

            var children = node.Children ?? Array.Empty<CreateBranchServiceTreeNodeCommand>();

            if (children.Count > 0 &&
                node.IsTicketIssuable.GetValueOrDefault())
            {
                return new Error(
                    "BranchServiceTrees.Create.TicketIssuableCannotHaveChildren",
                    ServiceFeatureMessages.BranchServiceTreeTicketIssuableCannotHaveChildren,
                    ErrorType.Conflict);
            }

            var duplicateSibling = ValidateSiblingNames(children);
            if (duplicateSibling is not null)
            {
                return duplicateSibling;
            }

            foreach (var child in children)
            {
                stack.Push(child);
            }
        }

        return null;
    }

    private static Error? ValidateNodeFields(
        CreateBranchServiceTreeNodeCommand node)
    {
        if (string.IsNullOrWhiteSpace(node.ArabicName))
        {
            return new Error(
                "BranchServiceTrees.Create.ArabicNameRequired",
                ServiceFeatureMessages.ArabicNameRequired,
                ErrorType.Validation);
        }

        if (node.ArabicName.Length > 100)
        {
            return new Error(
                "BranchServiceTrees.Create.ArabicNameMaxLength",
                ServiceFeatureMessages.ArabicNameMaxLength,
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(node.EnglishName))
        {
            return new Error(
                "BranchServiceTrees.Create.EnglishNameRequired",
                ServiceFeatureMessages.EnglishNameRequired,
                ErrorType.Validation);
        }

        if (node.EnglishName.Length > 100)
        {
            return new Error(
                "BranchServiceTrees.Create.EnglishNameMaxLength",
                ServiceFeatureMessages.EnglishNameMaxLength,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.ArabicUserMessage) &&
            node.ArabicUserMessage.Length > 500)
        {
            return new Error(
                "BranchServiceTrees.Create.ArabicUserMessageMaxLength",
                ServiceFeatureMessages.ArabicUserMessageMaxLength,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.EnglishUserMessage) &&
            node.EnglishUserMessage.Length > 500)
        {
            return new Error(
                "BranchServiceTrees.Create.EnglishUserMessageMaxLength",
                ServiceFeatureMessages.EnglishUserMessageMaxLength,
                ErrorType.Validation);
        }

        if (!node.IsTicketIssuable.HasValue)
        {
            return new Error(
                "BranchServiceTrees.Create.IsTicketIssuableRequired",
                ServiceFeatureMessages.IsTicketIssuableRequired,
                ErrorType.Validation);
        }

        if (node.OrderNo < 0)
        {
            return new Error(
                "BranchServiceTrees.Create.OrderNoNonNegative",
                ServiceFeatureMessages.OrderNoNonNegative,
                ErrorType.Validation);
        }

        if (node.Priority < 0)
        {
            return new Error(
                "BranchServiceTrees.Create.PriorityNonNegative",
                ServiceFeatureMessages.PriorityNonNegative,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.RangePrefix) &&
            node.RangePrefix.Length > 10)
        {
            return new Error(
                "BranchServiceTrees.Create.RangePrefixMaxLength",
                ServiceFeatureMessages.RangePrefixMaxLength,
                ErrorType.Validation);
        }

        if (node.RangeStartNumber is < 0)
        {
            return new Error(
                "BranchServiceTrees.Create.RangeStartNonNegative",
                ServiceFeatureMessages.RangeStartNonNegative,
                ErrorType.Validation);
        }

        if (node.RangeEndNumber.HasValue &&
            node.RangeStartNumber.HasValue &&
            node.RangeEndNumber.Value < node.RangeStartNumber.Value)
        {
            return new Error(
                "BranchServiceTrees.Create.RangeEndGreaterOrEqualStart",
                ServiceFeatureMessages.RangeEndGreaterOrEqualStart,
                ErrorType.Validation);
        }

        if (node.WaitingDuration is < 0)
        {
            return new Error(
                "BranchServiceTrees.Create.WaitingDurationNonNegative",
                ServiceFeatureMessages.WaitingDurationNonNegative,
                ErrorType.Validation);
        }

        if (node.NoOfTicketCopies is <= 0)
        {
            return new Error(
                "BranchServiceTrees.Create.NoOfTicketCopiesPositive",
                ServiceFeatureMessages.NoOfTicketCopiesPositive,
                ErrorType.Validation);
        }

        return ServiceRuleChecks.ValidateTicketSettings(
            node.IsTicketIssuable.GetValueOrDefault(),
            node.RangePrefix,
            node.RangeStartNumber,
            node.RangeEndNumber,
            node.WaitingDuration,
            node.NoOfTicketCopies,
            operation: "Create");
    }

    private static Error? ValidateSiblingNames(
        IReadOnlyCollection<CreateBranchServiceTreeNodeCommand> siblings)
    {
        var arabicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var englishNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sibling in siblings)
        {
            if (!arabicNames.Add(sibling.ArabicName.Trim()))
            {
                return new Error(
                    "BranchServiceTrees.Create.DuplicateArabicName",
                    ServiceFeatureMessages.BranchServiceTreeDuplicateArabicName,
                    ErrorType.Conflict);
            }

            if (!englishNames.Add(sibling.EnglishName.Trim()))
            {
                return new Error(
                    "BranchServiceTrees.Create.DuplicateEnglishName",
                    ServiceFeatureMessages.BranchServiceTreeDuplicateEnglishName,
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    private static CreatedNode BuildEntities(
        CreateBranchServiceTreeNodeCommand requestNode,
        Service? parentService,
        int branchId,
        Guid createdByApplicationUserId,
        ICollection<Service> services,
        ICollection<BranchService> assignments)
    {
        var service = Service.CreateBranchScoped(
            parentService,
            branchId,
            requestNode.ArabicName,
            requestNode.EnglishName,
            requestNode.ArabicUserMessage,
            requestNode.EnglishUserMessage,
            requestNode.IsTicketIssuable.GetValueOrDefault(),
            requestNode.IsClientInputRequired,
            requestNode.HasReservation,
            requestNode.OrderNo,
            requestNode.Priority,
            requestNode.RangePrefix,
            requestNode.RangeStartNumber,
            requestNode.RangeEndNumber,
            requestNode.WaitingDuration,
            requestNode.NoOfTicketCopies,
            createdByApplicationUserId);

        services.Add(service);
        assignments.Add(BranchService.Create(
            branchId,
            service,
            createdByApplicationUserId));

        var createdNode = new CreatedNode(service);

        foreach (var child in requestNode.Children ??
                 Array.Empty<CreateBranchServiceTreeNodeCommand>())
        {
            createdNode.Children.Add(BuildEntities(
                child,
                service,
                branchId,
                createdByApplicationUserId,
                services,
                assignments));
        }

        return createdNode;
    }

    private static CreatedBranchServiceTreeNodeResponse ToResponse(
        CreatedNode node)
    {
        return new CreatedBranchServiceTreeNodeResponse
        {
            Id = node.Service.Id,
            ParentServiceId = node.Service.ParentServiceId,
            ArabicName = node.Service.ArabicName,
            EnglishName = node.Service.EnglishName,
            Scope = node.Service.Scope,
            OwnerBranchId = node.Service.OwnerBranchId.GetValueOrDefault(),
            IsTicketIssuable = node.Service.IsTicketIssuable,
            Children = node.Children
                .Select(ToResponse)
                .ToList()
        };
    }

    private static Result<CreateBranchServiceTreeResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<CreateBranchServiceTreeResponse>.Fail(
            new Error(code, message, type));

    private sealed class CreatedNode
    {
        public CreatedNode(Service service)
        {
            Service = service;
        }

        public Service Service { get; }

        public List<CreatedNode> Children { get; } = new();
    }
}
