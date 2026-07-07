using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowsForService;

internal sealed class GetServiceWorkflowsForServiceQueryHandler
    : IQueryHandler<GetServiceWorkflowsForServiceQuery, ServiceWorkflowForServiceResponse>
{
    private readonly IWriteReadRepository<ServiceWorkflow>
        _workflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _stepReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetServiceWorkflowsForServiceQueryHandler(
        IWriteReadRepository<ServiceWorkflow> workflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep> stepReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser)
    {
        _workflowReadRepository = workflowReadRepository
            ?? throw new ArgumentNullException(nameof(workflowReadRepository));
        _stepReadRepository = stepReadRepository
            ?? throw new ArgumentNullException(nameof(stepReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<ServiceWorkflowForServiceResponse>> Handle(
        GetServiceWorkflowsForServiceQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceWorkflowForServiceResponse>.Fail(new Error(
                "ServiceWorkflows.Authentication.Required",
                ServiceWorkflowMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(request.ServiceId),
            cancellationToken);

        if (service is null)
        {
            return Result<ServiceWorkflowForServiceResponse>.Fail(new Error(
                "ServiceWorkflows.ServiceWorkflows.ServiceNotFound",
                ServiceWorkflowMessages.ServiceNotFound,
                ErrorType.NotFound));
        }

        var serviceItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var serviceStates = ServiceHierarchyCalculator.ComputeStates(
            serviceItems);
        var servicesById = serviceItems.ToDictionary(x => x.Id);

        var workflowsByService =
            await ServiceWorkflowReadHelpers.LoadContainingWorkflowsByServiceAsync(
                _workflowReadRepository,
                _stepReadRepository,
                new[] { request.ServiceId },
                servicesById,
                serviceStates,
                cancellationToken);

        workflowsByService.TryGetValue(
            request.ServiceId,
            out var workflows);

        return Result<ServiceWorkflowForServiceResponse>.Ok(
            new ServiceWorkflowForServiceResponse
            {
                ServiceId = service.Id,
                ArabicName = service.ArabicName,
                EnglishName = service.EnglishName,
                Workflows = workflows ??
                    Array.Empty<ServiceWorkflowContainingServiceResponse>()
            });
    }
}
