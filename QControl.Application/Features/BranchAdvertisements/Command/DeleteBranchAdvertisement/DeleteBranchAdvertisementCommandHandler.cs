using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchAdvertisements.Command.Shared;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.DeleteBranchAdvertisement;

internal sealed class DeleteBranchAdvertisementCommandHandler
    : ICommandHandler<DeleteBranchAdvertisementCommand, BranchAdvertisementDeleteResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchAdvertisement> _advertisementReadRepository;
    private readonly IWriteRepository<BranchAdvertisement> _advertisementWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBranchAdvertisementCommandHandler> _logger;

    public DeleteBranchAdvertisementCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchAdvertisement> advertisementReadRepository,
        IWriteRepository<BranchAdvertisement> advertisementWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteBranchAdvertisementCommandHandler> logger)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _advertisementReadRepository = advertisementReadRepository
            ?? throw new ArgumentNullException(nameof(advertisementReadRepository));
        _advertisementWriteRepository = advertisementWriteRepository
            ?? throw new ArgumentNullException(nameof(advertisementWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<BranchAdvertisementDeleteResponse>> Handle(
        DeleteBranchAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await LoadForDeleteAsync(
            request.BranchId,
            request.AdvertisementId,
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<BranchAdvertisementDeleteResponse>.Fail(
                loaded.Errors);
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<BranchAdvertisementDeleteResponse>.Fail(new Error(
                "BranchAdvertisements.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var advertisement = loaded.Value;
        var imagePath = advertisement.ImagePath;

        _concurrencyTokenManager.SetOriginalRowVersion(advertisement, rowVersion);
        _advertisementWriteRepository.Delete(advertisement);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<BranchAdvertisementDeleteResponse>.Fail(new Error(
                "BranchAdvertisements.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        try
        {
            _mediaService.Remove(imagePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Advertisement media cleanup failed for branch {BranchId}, advertisement {AdvertisementId}.",
                request.BranchId,
                request.AdvertisementId);
        }

        return Result<BranchAdvertisementDeleteResponse>.Ok(
            new BranchAdvertisementDeleteResponse
            {
                AdvertisementId = request.AdvertisementId,
                BranchId = request.BranchId,
                Message = BranchFeatureMessages.AdvertisementDeleted
            });
    }

    private async Task<Result<BranchAdvertisement>> LoadForDeleteAsync(
        int branchId,
        int advertisementId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchAdvertisement>.Fail(new Error(
                "BranchAdvertisements.Delete.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == branchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<BranchAdvertisement>.Fail(new Error(
                "BranchAdvertisements.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var advertisement = await _advertisementReadRepository.FirstOrDefaultAsync(
            new GetBranchAdvertisementForMutationSpec(advertisementId),
            cancellationToken);

        if (advertisement is null)
        {
            return Result<BranchAdvertisement>.Fail(new Error(
                "BranchAdvertisements.NotFound",
                BranchFeatureMessages.AdvertisementNotFound,
                ErrorType.NotFound));
        }

        if (advertisement.BranchId != branchId)
        {
            return Result<BranchAdvertisement>.Fail(new Error(
                "BranchAdvertisements.DoesNotBelongToBranch",
                BranchFeatureMessages.AdvertisementOwnershipMismatch,
                ErrorType.Conflict));
        }

        return Result<BranchAdvertisement>.Ok(advertisement);
    }
}
