using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
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
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteRepository<ServiceWorkflow> workflowWriteRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
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
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
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

        var duplicateError =
            await ServiceWorkflowRuleChecks.ValidateDuplicateNamesAsync(
                _workflowReadRepository,
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
            await ServiceWorkflowRuleChecks.ValidateStepServicesAreEligibleAsync(
                _serviceReadRepository,
                normalizedSteps,
                operation: "Create",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowResponse>.Fail(serviceError);
        }

        var workflow = ServiceWorkflow.Create(
            normalizedArabicName,
            normalizedEnglishName,
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
