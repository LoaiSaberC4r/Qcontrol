using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.ServiceSchedules.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

internal sealed class PermanentDeleteBranchCommandHandler
    : ICommandHandler<PermanentDeleteBranchCommand, PermanentDeleteBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IWriteRepository<Location> _locationWriteRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<ServiceWorkflow>?
        _serviceWorkflowReadRepository;
    private readonly IWriteReadRepository<ServiceSchedule>?
        _serviceScheduleReadRepository;
    private readonly IWriteReadRepository<QControl.Domain.Entities.BranchBranding>? _brandingReadRepository;
    private readonly IWriteRepository<QControl.Domain.Entities.BranchBranding>? _brandingWriteRepository;
    private readonly IWriteReadRepository<BranchAdvertisement>? _advertisementReadRepository;
    private readonly IWriteRepository<BranchAdvertisement>? _advertisementWriteRepository;
    private readonly IWriteReadRepository<BranchVideo>? _branchVideoReadRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediaService? _mediaService;
    private readonly ILogger<PermanentDeleteBranchCommandHandler>? _logger;

    public PermanentDeleteBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        IWriteRepository<Location> locationWriteRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Display> displayReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IWriteReadRepository<QControl.Domain.Entities.BranchBranding>? brandingReadRepository = null,
        IWriteRepository<QControl.Domain.Entities.BranchBranding>? brandingWriteRepository = null,
        IWriteReadRepository<BranchAdvertisement>? advertisementReadRepository = null,
        IWriteRepository<BranchAdvertisement>? advertisementWriteRepository = null,
        IMediaService? mediaService = null,
        ILogger<PermanentDeleteBranchCommandHandler>? logger = null,
        IWriteReadRepository<ServiceWorkflow>? serviceWorkflowReadRepository = null,
        IWriteReadRepository<ServiceSchedule>? serviceScheduleReadRepository = null,
        IWriteReadRepository<BranchVideo>? branchVideoReadRepository = null)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchWriteRepository = branchWriteRepository
            ?? throw new ArgumentNullException(nameof(branchWriteRepository));
        _locationWriteRepository = locationWriteRepository
            ?? throw new ArgumentNullException(nameof(locationWriteRepository));
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));
        _serviceWorkflowReadRepository = serviceWorkflowReadRepository;
        _serviceScheduleReadRepository = serviceScheduleReadRepository;
        _brandingReadRepository = brandingReadRepository;
        _brandingWriteRepository = brandingWriteRepository;
        _advertisementReadRepository = advertisementReadRepository;
        _advertisementWriteRepository = advertisementWriteRepository;
        _branchVideoReadRepository = branchVideoReadRepository;
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediaService = mediaService;
        _logger = logger;
    }

    public async Task<Result<PermanentDeleteBranchResponse>> Handle(
        PermanentDeleteBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchForDeleteSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        if (branch.IsActive)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.MustBeInactive",
                ErrorMessage.PermanentDelete_RequiresInactive,
                ErrorType.Conflict));
        }

        var hasWaitingAreas = await _waitingAreaReadRepository.AnyAsync(
            x => x.BranchId == branch.Id,
            cancellationToken);

        var hasDisplays = await _displayReadRepository.AnyAsync(
            x => x.BranchId == branch.Id,
            cancellationToken);

        var hasServiceWorkflows =
            _serviceWorkflowReadRepository is not null &&
            await _serviceWorkflowReadRepository.AnyAsync(
                x => x.BranchId == branch.Id,
                cancellationToken);

        var hasServiceSchedules =
            _serviceScheduleReadRepository is not null &&
            await _serviceScheduleReadRepository.AnyAsync(
                x => x.BranchId == branch.Id,
                cancellationToken);

        var hasBranchVideos =
            _branchVideoReadRepository is not null &&
            await _branchVideoReadRepository.AnyAsync(
                x => x.BranchId == branch.Id,
                cancellationToken);

        if (hasServiceSchedules)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.DeletePermanent.ServiceSchedulesExist",
                ServiceScheduleMessages.BranchPermanentDeleteServiceSchedulesExist,
                ErrorType.Conflict));
        }

        if (hasWaitingAreas || hasDisplays || hasServiceWorkflows || hasBranchVideos)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.HasRelatedData",
                ErrorMessage.Branch_PermanentDelete_HasRelatedData,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(branch, rowVersion);

        var branding = _brandingReadRepository is null
            ? null
            : await _brandingReadRepository.FirstOrDefaultAsync(
                new GetBranchBrandingForPermanentDeleteSpec(branch.Id),
                cancellationToken);

        var advertisements = _advertisementReadRepository is null
            ? new List<BranchAdvertisement>()
            : await _advertisementReadRepository.ListAsync(
                new GetBranchAdvertisementsForPermanentDeleteSpec(branch.Id),
                cancellationToken);

        var mediaPaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(branding?.LogoPath))
        {
            mediaPaths.Add(branding.LogoPath);
        }

        mediaPaths.AddRange(advertisements
            .Select(x => x.ImagePath)
            .Where(path => !string.IsNullOrWhiteSpace(path)));

        if (advertisements.Count > 0)
        {
            _advertisementWriteRepository?.DeleteRange(advertisements);
        }

        if (branding is not null)
        {
            _brandingWriteRepository?.Delete(branding);
        }

        if (branch.Location is not null)
        {
            _locationWriteRepository.Delete(branch.Location);
        }

        _branchWriteRepository.Delete(branch);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.HasRelatedData",
                ErrorMessage.Branch_PermanentDelete_HasRelatedData,
                ErrorType.Conflict));
        }

        if (_mediaService is not null && mediaPaths.Count > 0)
        {
            try
            {
                _mediaService.RemoveRange(mediaPaths);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "Branch branding media cleanup failed after permanent delete for branch {BranchId}.",
                    request.BranchId);
            }
        }

        return Result<PermanentDeleteBranchResponse>.Ok(
            new PermanentDeleteBranchResponse
            {
                BranchId = request.BranchId,
                Message = ErrorMessage.Branch_PermanentDelete_Success
            });
    }
}
