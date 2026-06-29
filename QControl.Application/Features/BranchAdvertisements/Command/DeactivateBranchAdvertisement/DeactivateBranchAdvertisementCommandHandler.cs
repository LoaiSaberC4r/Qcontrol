using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchAdvertisements.Command.Shared;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.DeactivateBranchAdvertisement;

internal sealed class DeactivateBranchAdvertisementCommandHandler
    : ICommandHandler<DeactivateBranchAdvertisementCommand, BranchAdvertisementStateResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchAdvertisement> _advertisementReadRepository;
    private readonly IWriteRepository<BranchAdvertisement> _advertisementWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchAdvertisementCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchAdvertisement> advertisementReadRepository,
        IWriteRepository<BranchAdvertisement> advertisementWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
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
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<BranchAdvertisementStateResponse>> Handle(
        DeactivateBranchAdvertisementCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await LoadForMutationAsync(
            request.BranchId,
            request.AdvertisementId,
            cancellationToken);

        if (loaded.IsFailure)
        {
            return Result<BranchAdvertisementStateResponse>.Fail(
                loaded.Errors);
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return InvalidRowVersion();
        }

        var advertisement = loaded.Value;

        if (!advertisement.IsActive)
        {
            return Result<BranchAdvertisementStateResponse>.Fail(new Error(
                "BranchAdvertisements.AlreadyInactive",
                BranchFeatureMessages.AlreadyInactive,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(advertisement, rowVersion);

        advertisement.Deactivate(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId!.Value);

        _advertisementWriteRepository.Update(advertisement);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConcurrencyConflict();
        }

        return Result<BranchAdvertisementStateResponse>.Ok(
            BranchAdvertisementResponseFactory.StateFromEntity(
                advertisement,
                BranchFeatureMessages.AdvertisementDeactivated));
    }

    private async Task<Result<BranchAdvertisement>> LoadForMutationAsync(
        int branchId,
        int advertisementId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchAdvertisement>.Fail(new Error(
                "BranchAdvertisements.Deactivate.Unauthenticated",
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

    private static Result<BranchAdvertisementStateResponse> InvalidRowVersion() =>
        Result<BranchAdvertisementStateResponse>.Fail(new Error(
            "BranchAdvertisements.InvalidRowVersion",
            ErrorMessage.RowVersion_Invalid,
            ErrorType.Validation));

    private static Result<BranchAdvertisementStateResponse> ConcurrencyConflict() =>
        Result<BranchAdvertisementStateResponse>.Fail(new Error(
            "BranchAdvertisements.ConcurrencyConflict",
            ErrorMessage.Concurrency_Conflict,
            ErrorType.Conflict));
}
