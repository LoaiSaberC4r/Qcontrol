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

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.DeactivateServiceWorkflow;

internal sealed class DeactivateServiceWorkflowCommandHandler
    : ICommandHandler<DeactivateServiceWorkflowCommand, ServiceWorkflowActivationResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteRepository<ServiceWorkflow>
        _workflowWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _workflowWriteRepository = workflowWriteRepository
            ?? throw new ArgumentNullException(nameof(workflowWriteRepository));
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
        DeactivateServiceWorkflowCommand request,
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
                "ServiceWorkflows.Deactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var workflow = await _workflowReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (workflow is null)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Deactivate.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound));
        }

        if (!workflow.IsActive)
        {
            return Result<ServiceWorkflowActivationResponse>.Fail(new Error(
                "ServiceWorkflows.Deactivate.AlreadyInactive",
                ServiceWorkflowMessages.AlreadyInactive,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            workflow,
            rowVersion);

        workflow.Deactivate(
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
                "ServiceWorkflows.Deactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ServiceWorkflowActivationResponse>.Ok(
            new ServiceWorkflowActivationResponse
            {
                WorkflowId = workflow.Id,
                IsActive = workflow.IsActive,
                RowVersion = RowVersionConverter.ToBase64(workflow.RowVersion),
                Message = ServiceWorkflowMessages.DeactivateSuccess
            });
    }
}
