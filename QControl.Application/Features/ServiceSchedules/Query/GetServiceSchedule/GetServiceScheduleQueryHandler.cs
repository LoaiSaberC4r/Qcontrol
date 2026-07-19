using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceSchedules.Query.GetServiceSchedule;

internal sealed class GetServiceScheduleQueryHandler
    : IQueryHandler<GetServiceScheduleQuery, ServiceScheduleResponse>
{
    private readonly IWriteReadRepository<ServiceSchedule>
        _scheduleReadRepository;
    private readonly IWriteReadRepository<Service>
        _serviceReadRepository;
    private readonly IWriteReadRepository<Branch>
        _branchReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IServiceVisibilityPolicy _serviceVisibilityPolicy;

    public GetServiceScheduleQueryHandler(
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IServiceVisibilityPolicy serviceVisibilityPolicy)
    {
        _scheduleReadRepository = scheduleReadRepository
            ?? throw new ArgumentNullException(nameof(scheduleReadRepository));
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
        _serviceVisibilityPolicy = serviceVisibilityPolicy
            ?? throw new ArgumentNullException(nameof(serviceVisibilityPolicy));
    }

    public async Task<Result<ServiceScheduleResponse>> Handle(
        GetServiceScheduleQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Get.Unauthenticated",
                ServiceScheduleMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var branchResult =
            await ServiceScheduleRuleChecks.ValidateBranchAccessAndStateAsync(
                _branchReadRepository,
                _branchAccessValidator,
                request.BranchId,
                "Get",
                requireActive: false,
                cancellationToken);

        if (branchResult.IsFailure)
        {
            return Result<ServiceScheduleResponse>.Fail(branchResult.Errors);
        }

        var serviceVisibilityError =
            await ServiceScheduleRuleChecks.ValidateServiceCanBeViewedAsync(
                _serviceReadRepository,
                _serviceVisibilityPolicy,
                request.LeafServiceId,
                "Get",
                cancellationToken);

        if (serviceVisibilityError is not null)
        {
            return Result<ServiceScheduleResponse>.Fail(
                serviceVisibilityError);
        }

        var response =
            await ServiceScheduleRuleChecks.BuildResponseAsync(
                _scheduleReadRepository,
                _serviceReadRepository,
                _branchServiceReadRepository,
                branchResult.Value,
                request.LeafServiceId,
                scheduleId: null,
                message: null,
                cancellationToken);

        return response is null
            ? Result<ServiceScheduleResponse>.Fail(new Error(
                "ServiceSchedules.Get.ScheduleNotFound",
                ServiceScheduleMessages.ScheduleNotFound,
                ErrorType.NotFound))
            : Result<ServiceScheduleResponse>.Ok(response);
    }
}
