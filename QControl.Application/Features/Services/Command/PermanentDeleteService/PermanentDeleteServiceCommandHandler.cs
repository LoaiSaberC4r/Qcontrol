using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.Services.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Command.PermanentDeleteService;

internal sealed class PermanentDeleteServiceCommandHandler
    : ICommandHandler<
        PermanentDeleteServiceCommand,
        PermanentDeleteServiceResponse>
{
    private const string CodePrefix = "Services.PermanentDelete";

    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;
    private readonly IWriteRepository<BranchService>
        _branchServiceWriteRepository;
    private readonly IWriteReadRepository<ServiceSchedule>
        _serviceScheduleReadRepository;
    private readonly IWriteRepository<ServiceSchedule>
        _serviceScheduleWriteRepository;
    private readonly IWriteReadRepository<ServiceImage>
        _serviceImageReadRepository;
    private readonly IWriteRepository<ServiceImage>
        _serviceImageWriteRepository;
    private readonly IWriteReadRepository<ServiceWorkflow>
        _serviceWorkflowReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflowStep>
        _serviceWorkflowStepReadRepository;
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _globalizationRequestReadRepository;
    private readonly IWriteReadRepository<ServiceGlobalizationRequestItem>
        _globalizationRequestItemReadRepository;
    private readonly IServicePermanentDeleteRepository
        _permanentDeleteRepository;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly IServiceDefinitionAccessValidator _accessValidator;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediaService _mediaService;
    private readonly ILogger<PermanentDeleteServiceCommandHandler> _logger;

    public PermanentDeleteServiceCommandHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteRepository<BranchService> branchServiceWriteRepository,
        IWriteReadRepository<ServiceSchedule> serviceScheduleReadRepository,
        IWriteRepository<ServiceSchedule> serviceScheduleWriteRepository,
        IWriteReadRepository<ServiceImage> serviceImageReadRepository,
        IWriteRepository<ServiceImage> serviceImageWriteRepository,
        IWriteReadRepository<ServiceWorkflow> serviceWorkflowReadRepository,
        IWriteReadRepository<ServiceWorkflowStep>
            serviceWorkflowStepReadRepository,
        IWriteReadRepository<ServiceGlobalizationRequest>
            globalizationRequestReadRepository,
        IWriteReadRepository<ServiceGlobalizationRequestItem>
            globalizationRequestItemReadRepository,
        IServicePermanentDeleteRepository permanentDeleteRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        IServiceDefinitionAccessValidator accessValidator,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMediaService mediaService,
        ILogger<PermanentDeleteServiceCommandHandler> logger)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(nameof(branchServiceReadRepository));
        _branchServiceWriteRepository = branchServiceWriteRepository
            ?? throw new ArgumentNullException(nameof(branchServiceWriteRepository));
        _serviceScheduleReadRepository = serviceScheduleReadRepository
            ?? throw new ArgumentNullException(nameof(serviceScheduleReadRepository));
        _serviceScheduleWriteRepository = serviceScheduleWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceScheduleWriteRepository));
        _serviceImageReadRepository = serviceImageReadRepository
            ?? throw new ArgumentNullException(nameof(serviceImageReadRepository));
        _serviceImageWriteRepository = serviceImageWriteRepository
            ?? throw new ArgumentNullException(nameof(serviceImageWriteRepository));
        _serviceWorkflowReadRepository = serviceWorkflowReadRepository
            ?? throw new ArgumentNullException(nameof(serviceWorkflowReadRepository));
        _serviceWorkflowStepReadRepository = serviceWorkflowStepReadRepository
            ?? throw new ArgumentNullException(nameof(serviceWorkflowStepReadRepository));
        _globalizationRequestReadRepository = globalizationRequestReadRepository
            ?? throw new ArgumentNullException(nameof(globalizationRequestReadRepository));
        _globalizationRequestItemReadRepository =
            globalizationRequestItemReadRepository
            ?? throw new ArgumentNullException(
                nameof(globalizationRequestItemReadRepository));
        _permanentDeleteRepository = permanentDeleteRepository
            ?? throw new ArgumentNullException(nameof(permanentDeleteRepository));
        _ticketUsageChecker = ticketUsageChecker
            ?? throw new ArgumentNullException(nameof(ticketUsageChecker));
        _accessValidator = accessValidator
            ?? throw new ArgumentNullException(nameof(accessValidator));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PermanentDeleteServiceResponse>> Handle(
        PermanentDeleteServiceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                $"{CodePrefix}.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Failure(
                $"{CodePrefix}.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation);
        }

        var service = await _serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceForMutationSpec(request.Id),
            cancellationToken);

        if (service is null)
        {
            return Failure(
                $"{CodePrefix}.ServiceNotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        if (service.Scope == ServiceScope.Global)
        {
            return Failure(
                $"{CodePrefix}.GlobalServiceNotSupported",
                ServiceFeatureMessages.PermanentDeleteGlobalNotSupported,
                ErrorType.Conflict);
        }

        var access = _accessValidator.EnsureCanEdit(service, CodePrefix);
        if (access.IsFailure)
        {
            if (access.Errors.Any(error =>
                    error.Code.EndsWith(
                        ".ForeignBranchServiceForbidden",
                        StringComparison.Ordinal)))
            {
                return Failure(
                    $"{CodePrefix}.OwnershipMismatch",
                    ServiceFeatureMessages.ForeignBranchServiceForbidden,
                    ErrorType.Security);
            }

            return Result<PermanentDeleteServiceResponse>.Fail(access.Errors);
        }

        if (!service.OwnerBranchId.HasValue)
        {
            return Failure(
                $"{CodePrefix}.OwnershipMismatch",
                ServiceFeatureMessages.ForeignBranchServiceForbidden,
                ErrorType.Security);
        }

        request.OwnerBranchId = service.OwnerBranchId.Value;

        if (!service.IsDeleted)
        {
            return Failure(
                $"{CodePrefix}.MustBeSoftDeletedFirst",
                ServiceFeatureMessages
                    .PermanentDeleteMustBeSoftDeletedFirst,
                ErrorType.Conflict);
        }

        var hasHistoricalTickets =
            await _ticketUsageChecker.HasHistoricalTicketsAsync(
                service.Id,
                cancellationToken);

        if (hasHistoricalTickets)
        {
            return Failure(
                $"{CodePrefix}.HasHistoricalTickets",
                ServiceFeatureMessages
                    .PermanentDeleteHasHistoricalTickets,
                ErrorType.Conflict);
        }

        var childIds = await _serviceReadRepository.ListAsync(
            new ServiceHasChildrenSpec(service.Id),
            cancellationToken);

        if (childIds.Count > 0)
        {
            return Failure(
                $"{CodePrefix}.HasChildren",
                ServiceFeatureMessages.PermanentDeleteHasChildren,
                ErrorType.Conflict);
        }

        var hasWorkflowReferences =
            await _serviceWorkflowReadRepository.AnyAsync(
                workflow => workflow.LeafServiceId == service.Id,
                cancellationToken) ||
            await _serviceWorkflowStepReadRepository.AnyAsync(
                step => step.ServiceId == service.Id,
                cancellationToken);

        if (hasWorkflowReferences)
        {
            return Failure(
                $"{CodePrefix}.HasWorkflowReferences",
                ServiceFeatureMessages
                    .PermanentDeleteHasWorkflowReferences,
                ErrorType.Conflict);
        }

        var hasPendingGlobalizationRequest =
            await _globalizationRequestReadRepository.AnyAsync(
                globalizationRequest =>
                    globalizationRequest.RootServiceId == service.Id &&
                    globalizationRequest.Status ==
                        ServiceGlobalizationRequestStatus.Pending,
                cancellationToken) ||
            await _globalizationRequestItemReadRepository.AnyAsync(
                item =>
                    item.ServiceId == service.Id &&
                    item.Request.Status ==
                        ServiceGlobalizationRequestStatus.Pending,
                cancellationToken);

        if (hasPendingGlobalizationRequest)
        {
            return Failure(
                $"{CodePrefix}.HasPendingGlobalizationRequest",
                ServiceFeatureMessages
                    .PermanentDeleteHasPendingGlobalizationRequest,
                ErrorType.Conflict);
        }

        var assignments = await _branchServiceReadRepository.ListAsync(
            new GetOwnedBranchServiceForPermanentDeleteSpec(
                service.OwnerBranchId.Value,
                service.Id),
            cancellationToken);
        var schedules = await _serviceScheduleReadRepository.ListAsync(
            new GetServiceSchedulesForPermanentDeleteSpec(service.Id),
            cancellationToken);
        var images = await _serviceImageReadRepository.ListAsync(
            new GetServiceImagesForPermanentDeleteSpec(service.Id),
            cancellationToken);
        var mediaPaths = images
            .Select(image => image.ImagePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _concurrencyTokenManager.SetOriginalRowVersion(service, rowVersion);

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (images.Count > 0)
            {
                _serviceImageWriteRepository.DeleteRange(images);
            }

            if (schedules.Count > 0)
            {
                _serviceScheduleWriteRepository.DeleteRange(schedules);
            }

            if (assignments.Count > 0)
            {
                _branchServiceWriteRepository.DeleteRange(assignments);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _permanentDeleteRepository.DeletePermanentlyAsync(
                service.Id,
                rowVersion,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ConcurrencyFailure();
        }
        catch (ServicePermanentDeleteConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ConcurrencyFailure();
        }
        catch (ServicePermanentDeleteConflictException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return DependencyFailure();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return DependencyFailure();
        }

        if (mediaPaths.Length > 0)
        {
            try
            {
                _mediaService.RemoveRange(mediaPaths);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Service media cleanup failed after permanent delete for service {ServiceId}.",
                    service.Id);
            }
        }

        return Result<PermanentDeleteServiceResponse>.Ok(
            new PermanentDeleteServiceResponse
            {
                ServiceId = service.Id,
                Message = ServiceFeatureMessages.PermanentDeleteSuccess
            });
    }

    private static Result<PermanentDeleteServiceResponse>
        ConcurrencyFailure()
        => Failure(
            $"{CodePrefix}.ConcurrencyConflict",
            ErrorMessage.Concurrency_Conflict,
            ErrorType.Conflict);

    private static Result<PermanentDeleteServiceResponse>
        DependencyFailure()
        => Failure(
            $"{CodePrefix}.HasDependencies",
            ServiceFeatureMessages.PermanentDeleteHasDependencies,
            ErrorType.Conflict);

    private static Result<PermanentDeleteServiceResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<PermanentDeleteServiceResponse>.Fail(
            new Error(code, message, type));
}
