using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

internal sealed class UpdateBranchThemeCommandHandler
    : ICommandHandler<UpdateBranchThemeCommand, BranchBrandingResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<QControl.Domain.Entities.BranchBranding> _brandingReadRepository;
    private readonly IWriteRepository<QControl.Domain.Entities.BranchBranding> _brandingWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchThemeCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<QControl.Domain.Entities.BranchBranding> brandingReadRepository,
        IWriteRepository<QControl.Domain.Entities.BranchBranding> brandingWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
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
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<BranchBrandingResponse>> Handle(
        UpdateBranchThemeCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.UpdateTheme.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
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

        if (!BranchBrandingColorNormalizer.TryNormalize(request.MainColor, out var mainColor) ||
            !BranchBrandingColorNormalizer.TryNormalize(request.SecondaryColor, out var secondaryColor) ||
            !BranchBrandingColorNormalizer.TryNormalize(request.BackgroundColor, out var backgroundColor) ||
            !OptionalColorsAreValid(request))
        {
            return Result<BranchBrandingResponse>.Fail(new Error(
                "BranchBranding.InvalidColor",
                BranchFeatureMessages.InvalidColor,
                ErrorType.Validation));
        }

        var branding = await _brandingReadRepository.FirstOrDefaultAsync(
            new GetBranchBrandingForUpdateSpec(request.BranchId),
            cancellationToken);
        var layout = BrandingLayoutSettingsFactory.FromInput(request);

        if (branding is null)
        {
            if (!string.IsNullOrWhiteSpace(request.RowVersion))
            {
                return InvalidRowVersion();
            }

            branding = QControl.Domain.Entities.BranchBranding.Create(
                request.BranchId,
                _currentUser.UserId.Value);

            branding.UpdateLayout(
                layout,
                _currentUser.UserId.Value);

            await _brandingWriteRepository.AddAsync(
                branding,
                cancellationToken);
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

            branding.UpdateLayout(
                layout,
                _currentUser.UserId.Value);

            _brandingWriteRepository.Update(branding);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConcurrencyConflict();
        }
        catch (DbUpdateException ex)
            when (BranchBrandingUniqueConstraintErrorMapper.TryMap(
                ex,
                out var error))
        {
            return Result<BranchBrandingResponse>.Fail(error);
        }

        return Result<BranchBrandingResponse>.Ok(
            BranchBrandingResponseFactory.FromEntity(
                branding,
                BranchFeatureMessages.ThemeSaved));
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

    private static bool OptionalColorsAreValid(
        UpdateBranchThemeCommand request)
    {
        var colors = new[]
        {
            request.HeaderColor,
            request.FooterColor,
            request.MainTextColor,
            request.LanguageButtonBackgroundColor,
            request.LanguageButtonTextColor,
            request.ServiceButtonBackgroundColor,
            request.ServiceButtonTextColor,
            request.KeypadButtonBackgroundColor,
            request.KeypadButtonTextColor,
            request.FooterButtonBackgroundColor,
            request.FooterButtonTextColor
        };

        return colors.All(color => color is null ||
            BranchBrandingColorNormalizer.TryNormalize(color, out _));
    }

}
