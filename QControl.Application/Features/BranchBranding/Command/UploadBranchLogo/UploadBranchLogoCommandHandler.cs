using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Validation;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;

internal sealed class UploadBranchLogoCommandHandler
    : ICommandHandler<UploadBranchLogoCommand, BranchBrandingResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<QControl.Domain.Entities.BranchBranding> _brandingReadRepository;
    private readonly IWriteRepository<QControl.Domain.Entities.BranchBranding> _brandingWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadBranchLogoCommandHandler> _logger;

    public UploadBranchLogoCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<QControl.Domain.Entities.BranchBranding> brandingReadRepository,
        IWriteRepository<QControl.Domain.Entities.BranchBranding> brandingWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork,
        ILogger<UploadBranchLogoCommandHandler> logger)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _brandingReadRepository = brandingReadRepository
            ?? throw new ArgumentNullException(nameof(brandingReadRepository));
        _brandingWriteRepository = brandingWriteRepository
            ?? throw new ArgumentNullException(nameof(brandingWriteRepository));
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

    public async Task<Result<BranchBrandingResponse>> Handle(
        UploadBranchLogoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.UploadLogo.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var validationError = BranchImageFileValidator.Validate(
            request.Logo,
            "BranchBranding.LogoRequired",
            BranchFeatureMessages.LogoRequired,
            "BranchBranding.InvalidImageType",
            BranchFeatureMessages.BrandingInvalidImageType);

        if (validationError is not null)
        {
            return Result<BranchBrandingResponse>.Fail(validationError);
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var branding = await _brandingReadRepository.FirstOrDefaultAsync(
            new GetBranchBrandingForUpdateSpec(request.BranchId),
            cancellationToken);

        if (branding is null)
        {
            if (!string.IsNullOrWhiteSpace(request.RowVersion))
            {
                return InvalidRowVersion();
            }
        }
        else
        {
            if (!RowVersionConverter.TryDecode(
                    request.RowVersion,
                    out var rowVersion))
            {
                return InvalidRowVersion();
            }

            _concurrencyTokenManager.SetOriginalRowVersion(
                branding,
                rowVersion);
        }

        var newLogoPath = string.Empty;
        var oldLogoPath = branding?.LogoPath;

        try
        {
            newLogoPath = await _mediaService.SaveAsync(
                request.Logo!,
                $"Branches/{request.BranchId}/Logo");
        }
        catch
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.MediaSaveFailed",
                BranchFeatureMessages.BrandingMediaSaveFailed,
                ErrorType.Infrastructure));
        }

        if (branding is null)
        {
            branding = QControl.Domain.Entities.BranchBranding.Create(
                request.BranchId,
                _currentUser.UserId.Value);

            branding.ReplaceLogo(
                newLogoPath,
                _currentUser.UserId.Value);

            await _brandingWriteRepository.AddAsync(
                branding,
                cancellationToken);
        }
        else
        {
            branding.ReplaceLogo(
                newLogoPath,
                _currentUser.UserId.Value);

            _brandingWriteRepository.Update(branding);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            SafeRemoveNewLogo(newLogoPath);
            return ConcurrencyConflict();
        }
        catch (DbUpdateException ex)
            when (BranchBrandingUniqueConstraintErrorMapper.TryMap(
                ex,
                out var error))
        {
            SafeRemoveNewLogo(newLogoPath);
            return Result<BranchBrandingResponse>.Fail(error);
        }
        catch
        {
            SafeRemoveNewLogo(newLogoPath);
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.PersistenceFailed",
                BranchFeatureMessages.BrandingMediaSaveFailed,
                ErrorType.Infrastructure));
        }

        if (!string.IsNullOrWhiteSpace(oldLogoPath) &&
            !string.Equals(oldLogoPath, newLogoPath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _mediaService.Remove(oldLogoPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Old branch logo cleanup failed for branch {BranchId}.",
                    request.BranchId);
            }
        }

        return Result<BranchBrandingResponse>.Ok(
            BranchBrandingResponseFactory.FromEntity(
                branding,
                BranchFeatureMessages.LogoUploaded));
    }

    private void SafeRemoveNewLogo(string newLogoPath)
    {
        try
        {
            _mediaService.Remove(newLogoPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Compensating branch logo cleanup failed.");
        }
    }

    private static Result<BranchBrandingResponse> InvalidRowVersion() =>
        Result<BranchBrandingResponse>.Fail(new Error(
            "BranchBranding.InvalidRowVersion",
            ErrorMessage.RowVersion_Invalid,
            ErrorType.Validation));

    private static Result<BranchBrandingResponse> ConcurrencyConflict() =>
        Result<BranchBrandingResponse>.Fail(new Error(
            "BranchBranding.ConcurrencyConflict",
            ErrorMessage.Concurrency_Conflict,
            ErrorType.Conflict));
}
