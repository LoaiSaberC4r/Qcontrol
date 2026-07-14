using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.SetDefaultServiceWorkflow;

internal sealed class SetDefaultServiceWorkflowCommandHandler
    : ICommandHandler<SetDefaultServiceWorkflowCommand, ServiceWorkflowResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly IServiceWorkflowDefaultRepository _defaultRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SetDefaultServiceWorkflowCommandHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IServiceWorkflowDefaultRepository defaultRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IDateTimeProvider dateTimeProvider)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceReadRepository));
        _defaultRepository = defaultRepository
            ?? throw new ArgumentNullException(nameof(defaultRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<Result<ServiceWorkflowResponse>> Handle(
        SetDefaultServiceWorkflowCommand request,
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
                "ServiceWorkflows.SetDefault.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "SetDefault",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceWorkflowResponse>.Fail(branchResult.Errors);
        }

        var workflowState = await _workflowReadRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.Id == request.Id &&
                x.BranchId == request.BranchId &&
                x.LeafServiceId == request.LeafServiceId)
            .Select(x => new
            {
                x.Id,
                x.IsActive,
                x.IsDefault
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workflowState is null)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.SetDefault.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound));
        }

        if (!workflowState.IsActive)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.SetDefault.Inactive",
                ServiceWorkflowMessages.InactiveWorkflowCannotBecomeDefault,
                ErrorType.Conflict));
        }

        if (workflowState.IsDefault)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.SetDefault.AlreadyDefault",
                ServiceWorkflowMessages.WorkflowAlreadyDefault,
                ErrorType.Conflict));
        }

        var steps = await _stepReadRepository.Query()
            .AsNoTracking()
            .Where(x => x.ServiceWorkflowId == request.Id)
            .Select(x => new ServiceWorkflowStepCommandItem
            {
                ServiceId = x.ServiceId,
                StepOrder = x.StepOrder
            })
            .ToListAsync(cancellationToken);

        var serviceError =
            await ServiceWorkflowRuleChecks.ValidateOwnerAndStepServicesAsync(
                _serviceReadRepository,
                _branchServiceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                steps,
                "SetDefault",
                cancellationToken);

        if (serviceError is not null)
        {
            return Result<ServiceWorkflowResponse>.Fail(serviceError);
        }

        var setDefaultResult = await _defaultRepository.SetDefaultAsync(
            request.BranchId,
            request.LeafServiceId,
            request.Id,
            rowVersion,
            _currentUser.UserId.Value,
            _dateTimeProvider.UtcNow,
            cancellationToken);

        if (setDefaultResult.Status !=
            ServiceWorkflowSetDefaultStatus.Success)
        {
            return Result<ServiceWorkflowResponse>.Fail(
                MapFailure(setDefaultResult.Status));
        }

        var response =
            await ServiceWorkflowRuleChecks.BuildDetailsResponseAsync(
                _workflowReadRepository,
                _stepReadRepository,
                _serviceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                request.Id,
                ServiceWorkflowMessages.DefaultWorkflowChangedSuccessfully,
                cancellationToken);

        return response is null
            ? Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.SetDefault.NotFoundAfterSave",
                ServiceWorkflowMessages.NotFound,
                ErrorType.Infrastructure))
            : Result<ServiceWorkflowResponse>.Ok(response);
    }

    private static Error MapFailure(ServiceWorkflowSetDefaultStatus status)
        => status switch
        {
            ServiceWorkflowSetDefaultStatus.NotFound => new Error(
                "ServiceWorkflows.SetDefault.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound),

            ServiceWorkflowSetDefaultStatus.InvalidOwnership => new Error(
                "ServiceWorkflows.SetDefault.OwnershipMismatch",
                ServiceWorkflowMessages.WorkflowOwnershipMismatch,
                ErrorType.NotFound),

            ServiceWorkflowSetDefaultStatus.Inactive => new Error(
                "ServiceWorkflows.SetDefault.Inactive",
                ServiceWorkflowMessages.InactiveWorkflowCannotBecomeDefault,
                ErrorType.Conflict),

            ServiceWorkflowSetDefaultStatus.ConcurrencyConflict => new Error(
                "ServiceWorkflows.SetDefault.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict),

            ServiceWorkflowSetDefaultStatus.AlreadyDefault => new Error(
                "ServiceWorkflows.SetDefault.AlreadyDefault",
                ServiceWorkflowMessages.WorkflowAlreadyDefault,
                ErrorType.Conflict),

            _ => new Error(
                "ServiceWorkflows.SetDefault.PersistenceConflict",
                ServiceWorkflowMessages.FirstWorkflowDefaultConflict,
                ErrorType.Conflict)
        };
}
