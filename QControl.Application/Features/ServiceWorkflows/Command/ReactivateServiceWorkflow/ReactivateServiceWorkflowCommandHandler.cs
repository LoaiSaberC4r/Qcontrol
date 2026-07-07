using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.ReactivateServiceWorkflow;

internal sealed class ReactivateServiceWorkflowCommandHandler
    : ICommandHandler<ReactivateServiceWorkflowCommand, ServiceWorkflowActivationResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteRepository<ServiceWorkflow>
        _workflowWriteRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
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
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ServiceWorkflowActivationResponse>> Handle(
        ReactivateServiceWorkflowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Reactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var workflow = await _workflowReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (workflow is null)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Reactivate.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound));
        }

        if (workflow.IsActive)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Reactivate.AlreadyActive",
                ServiceWorkflowMessages.AlreadyActive,
                ErrorType.Conflict));
        }

        var steps = await _stepReadRepository.Query()
            .Where(x => x.ServiceWorkflowId == workflow.Id)
            .Select(x => new ServiceWorkflowStepCommandItem
            {
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToListAsync(cancellationToken);

        var serviceError =
            await ServiceWorkflowRuleChecks.ValidateStepServicesAreEligibleAsync(
                _serviceReadRepository,
                steps,
                operation: "Reactivate",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(serviceError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            workflow,
            rowVersion);

        workflow.Reactivate(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _workflowWriteRepository.Update(workflow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Reactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ServiceWorkflowActivationResponse>.Ok(
            new ServiceWorkflowActivationResponse
            {
                WorkflowId = workflow.Id,
                IsActive = workflow.IsActive,
                RowVersion = RowVersionConverter.ToBase64(workflow.RowVersion),
                Message = ServiceWorkflowMessages.ReactivateSuccess
            });
    }
}
