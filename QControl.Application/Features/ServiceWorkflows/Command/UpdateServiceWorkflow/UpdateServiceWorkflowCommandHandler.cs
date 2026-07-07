using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
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
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
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

        var workflow = await _workflowReadRepository.FirstOrDefaultAsync(
            new GetServiceWorkflowForMutationSpec(request.Id),
            cancellationToken);

        if (workflow is null)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Update.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound));
        }

        var normalizedArabicName = request.ArabicName.Trim();
        var normalizedEnglishName = request.EnglishName.Trim();
        var normalizedSteps = ServiceWorkflowRuleChecks.NormalizeSteps(
            request.Steps);

        var duplicateError =
            await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
                _workflowReadRepository,
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
            await ServiceWorkflowRuleChecks.ValidateStepServicesAreEligibleAsync(
                _serviceReadRepository,
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
