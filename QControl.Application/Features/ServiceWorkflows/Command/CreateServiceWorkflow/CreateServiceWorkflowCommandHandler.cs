using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;

internal sealed class CreateServiceWorkflowCommandHandler
    : ICommandHandler<CreateServiceWorkflowCommand, ServiceWorkflowResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteRepository<ServiceWorkflow>
        _workflowWriteRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IUnitOfWork unitOfWork)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _workflowWriteRepository = workflowWriteRepository
            ?? throw new ArgumentNullException(nameof(workflowWriteRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
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
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceWorkflowResponse>> Handle(
        CreateServiceWorkflowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedSteps = ServiceWorkflowRuleChecks.NormalizeSteps(
            request.Steps);

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "Create",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceWorkflowResponse>.Fail(branchResult.Errors);
        }

        var duplicateError =
            await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
                _workflowReadRepository,
                request.BranchId,
                request.LeafServiceId,
                normalizedArabicName,
                normalizedEnglishName,
                excludedWorkflowId: null,
                operation: "Create",
                cancellationToken);

        if (duplicateError is not null)
        {
            return Result<ServiceWorkflowResponse>.Fail(duplicateError);
        }

        var serviceError =
            await ServiceWorkflowRuleChecks.ValidateOwnerAndStepServicesAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                normalizedSteps,
                operation: "Create",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowResponse>.Fail(serviceError);
        }

        var hasExistingWorkflow = await _workflowReadRepository.Query()
            .AnyAsync(x =>
                x.BranchId == request.BranchId &&
                x.LeafServiceId == request.LeafServiceId,
                cancellationToken);

        var workflow = ServiceWorkflow.Create(
            request.BranchId,
            request.LeafServiceId,
            normalizedArabicName,
            normalizedEnglishName,
            isDefault: !hasExistingWorkflow,
            ServiceWorkflowRuleChecks.ToDomainSteps(normalizedSteps),
            _currentUser.UserId.Value);

        await _workflowWriteRepository.AddAsync(
            workflow,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ServiceWorkflowUniqueConstraintErrorMapper.TryMapCreate(
                ex,
                out var error))
        {
            return Result<ServiceWorkflowResponse>.Fail(error);
        }

        var response =
            await ServiceWorkflowRuleChecks.BuildDetailsResponseAsync(
                _workflowReadRepository,
                _stepReadRepository,
                _serviceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                workflow.Id,
                ServiceWorkflowMessages.CreateSuccess,
                cancellationToken);

        return response is null
            ? Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Create.NotFoundAfterSave",
                ServiceWorkflowMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceWorkflowResponse>.Ok(response);
    }
}
