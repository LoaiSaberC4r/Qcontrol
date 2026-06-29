using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;

internal sealed class AddBranchAdvertisementsCommandHandler
    : ICommandHandler<AddBranchAdvertisementsCommand, IReadOnlyList<BranchAdvertisementResponse>>
{
    private const int MaximumAdvertisementsPerBranch = 20;

    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchAdvertisement> _advertisementReadRepository;
    private readonly IWriteRepository<BranchAdvertisement> _advertisementWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddBranchAdvertisementsCommandHandler> _logger;

    public AddBranchAdvertisementsCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchAdvertisement> advertisementReadRepository,
        IWriteRepository<BranchAdvertisement> advertisementWriteRepository,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<AddBranchAdvertisementsCommandHandler> logger)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _advertisementReadRepository = advertisementReadRepository
            ?? throw new ArgumentNullException(nameof(advertisementReadRepository));
        _advertisementWriteRepository = advertisementWriteRepository
            ?? throw new ArgumentNullException(nameof(advertisementWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _mediaService = mediaService
            ?? throw new ArgumentNullException(nameof(mediaService));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IReadOnlyList<BranchAdvertisementResponse>>> Handle(
        AddBranchAdvertisementsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.Add.Unauthenticated",
                    ErrorMessage.Branch_Authentication_Required,
                    ErrorType.Unauthorized));
        }

        if (request.Images.Count == 0)
        {
            return ValidationFailure(
                "BranchAdvertisements.ImagesRequired",
                BranchFeatureMessages.AdvertisementImagesRequired);
        }

        foreach (var image in request.Images)
        {
            var validationError = BranchImageFileValidator.Validate(
                image,
                "BranchAdvertisements.ImagesRequired",
                BranchFeatureMessages.AdvertisementImagesRequired,
                "BranchAdvertisements.InvalidImageType",
                BranchFeatureMessages.AdvertisementInvalidImageType);

            if (validationError is not null)
            {
                return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                    validationError);
            }
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.BranchNotFound",
                    ErrorMessage.Branch_NotFound,
                    ErrorType.NotFound));
        }

        var currentCount = await _advertisementReadRepository.CountAsync(
            x => x.BranchId == request.BranchId,
            cancellationToken);

        if (currentCount + request.Images.Count > MaximumAdvertisementsPerBranch)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.MaximumExceeded",
                    BranchFeatureMessages.AdvertisementLimitExceeded,
                    ErrorType.Conflict));
        }

        var currentOrders = await _advertisementReadRepository.ListAsync(
            new GetCurrentBranchAdvertisementOrdersSpec(request.BranchId),
            cancellationToken);

        var availableOrders = Enumerable
            .Range(1, MaximumAdvertisementsPerBranch)
            .Except(currentOrders)
            .Take(request.Images.Count)
            .ToArray();

        var savedPaths = new List<string>();

        try
        {
            foreach (var image in request.Images)
            {
                savedPaths.Add(await _mediaService.SaveAsync(
                    image,
                    $"Branches/{request.BranchId}/Advertisements"));
            }
        }
        catch
        {
            SafeRemoveRange(savedPaths);
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.MediaSaveFailed",
                    BranchFeatureMessages.AdvertisementMediaSaveFailed,
                    ErrorType.Infrastructure));
        }

        var advertisements = savedPaths
            .Select((path, index) => BranchAdvertisement.Create(
                request.BranchId,
                path,
                availableOrders[index],
                _currentUser.UserId.Value))
            .ToList();

        await _advertisementWriteRepository.AddRangeAsync(
            advertisements,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BranchAdvertisementUniqueConstraintErrorMapper
                .TryMapOrderConflict(ex, out var error))
        {
            SafeRemoveRange(savedPaths);
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                error);
        }
        catch
        {
            SafeRemoveRange(savedPaths);
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.PersistenceFailed",
                    BranchFeatureMessages.AdvertisementMediaSaveFailed,
                    ErrorType.Infrastructure));
        }

        return Result<IReadOnlyList<BranchAdvertisementResponse>>.Ok(
            advertisements
                .OrderBy(x => x.DisplayOrder)
                .Select(BranchAdvertisementResponseFactory.FromEntity)
                .ToList());
    }

    private static Result<IReadOnlyList<BranchAdvertisementResponse>>
        ValidationFailure(string code, string message) =>
        Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
            new Error(code, message, ErrorType.Validation));

    private void SafeRemoveRange(IEnumerable<string> paths)
    {
        try
        {
            _mediaService.RemoveRange(paths);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Compensating advertisement media cleanup failed.");
        }
    }
}
