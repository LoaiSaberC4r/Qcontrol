using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;

internal sealed class UpdateServiceWorkflowCommandHandler
    : ICommandHandler<UpdateServiceWorkflowCommand, ServiceWorkflowResponse>
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
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
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
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceWorkflowResponse>> Handle(
        UpdateServiceWorkflowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "Update",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceWorkflowResponse>.Fail(branchResult.Errors);
        }

        var workflow = await _workflowReadRepository.FirstOrDefaultAsync(
            new GetServiceWorkflowForMutationSpec(
                request.BranchId,
                request.LeafServiceId,
                request.Id),
            cancellationToken);

        if (workflow is null)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Update.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound));
        }

        var normalizedArabicName = ServiceWorkflowNameNormalizer.Normalize(
            request.ArabicName);
        var normalizedEnglishName = ServiceWorkflowNameNormalizer.Normalize(
            request.EnglishName);
        var normalizedSteps = ServiceWorkflowRuleChecks.NormalizeSteps(
            request.Steps);

        var duplicateError =
            await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
                _workflowReadRepository,
                request.BranchId,
                request.LeafServiceId,
                normalizedArabicName,
                normalizedEnglishName,
                workflow.Id,
                operation: "Update",
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
                operation: "Update",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowResponse>.Fail(serviceError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            workflow,
            rowVersion);

        workflow.Update(
            normalizedArabicName,
            normalizedEnglishName,
            ServiceWorkflowRuleChecks.ToDomainSteps(normalizedSteps),
            _currentUser.UserId.Value);

        _workflowWriteRepository.Update(workflow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException ex)
            when (ServiceWorkflowUniqueConstraintErrorMapper.TryMapUpdate(
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
                ServiceWorkflowMessages.UpdateSuccess,
                cancellationToken);

        return response is null
            ? Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Update.NotFoundAfterSave",
                ServiceWorkflowMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceWorkflowResponse>.Ok(response);
    }
}
