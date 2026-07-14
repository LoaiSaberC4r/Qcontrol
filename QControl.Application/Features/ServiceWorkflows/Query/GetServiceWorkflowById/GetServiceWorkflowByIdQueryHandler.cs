using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;

internal sealed class GetServiceWorkflowByIdQueryHandler
    : IQueryHandler<GetServiceWorkflowByIdQuery, ServiceWorkflowResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetServiceWorkflowByIdQueryHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
    }

    public async Task<Result<ServiceWorkflowResponse>> Handle(
        GetServiceWorkflowByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var branchResult =
            await ServiceWorkflowRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "GetById",
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceWorkflowResponse>.Fail(branchResult.Errors);
        }

        var response =
            await ServiceWorkflowRuleChecks.BuildDetailsResponseAsync(
                _workflowReadRepository,
                _stepReadRepository,
                _serviceReadRepository,
                request.BranchId,
                request.LeafServiceId,
                request.Id,
                message: null,
                cancellationToken);

        return response is null
            ? Result<ServiceWorkflowResponse>.Fail(new Error(
                "ServiceWorkflows.GetById.NotFound",
                ServiceWorkflowMessages.NotFound,
                ErrorType.NotFound))
            : Result<ServiceWorkflowResponse>.Ok(response);
    }
}
