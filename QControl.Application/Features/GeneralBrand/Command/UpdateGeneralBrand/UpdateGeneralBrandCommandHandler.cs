using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;

internal sealed class UpdateGeneralBrandCommandHandler
    : ICommandHandler<UpdateGeneralBrandCommand, GeneralBrandResponse>
{
    private readonly IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandReadRepository;
    private readonly IWriteRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGeneralBrandCommandHandler(
        IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandReadRepository,
        IWriteRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _generalBrandReadRepository = generalBrandReadRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandReadRepository));
        _generalBrandWriteRepository = generalBrandWriteRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(
                nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<GeneralBrandResponse>> Handle(
        UpdateGeneralBrandCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GeneralBrandResponse>.Fail(new Error(
                "GeneralBrand.Update.Unauthenticated",
                GeneralBrandFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return InvalidRowVersion();
        }

        var generalBrand = await _generalBrandReadRepository.FirstOrDefaultAsync(
            new GetGeneralBrandForUpdateSpec(),
            cancellationToken);

        if (generalBrand is null)
        {
            return Result<GeneralBrandResponse>.Fail(new Error(
                "GeneralBrand.NotConfigured",
                GeneralBrandFeatureMessages.NotConfigured,
                ErrorType.NotFound));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            generalBrand,
            rowVersion);

        generalBrand.UpdateLayout(
            BrandingLayoutSettingsFactory.FromInput(request),
            _currentUser.UserId.Value);
        _generalBrandWriteRepository.Update(generalBrand);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConcurrencyConflict();
        }

        return Result<GeneralBrandResponse>.Ok(
            GeneralBrandResponseFactory.FromEntity(
                generalBrand,
                GeneralBrandFeatureMessages.Updated));
    }

    private static Result<GeneralBrandResponse> InvalidRowVersion() =>
        Result<GeneralBrandResponse>.Fail(new Error(
            "GeneralBrand.InvalidRowVersion",
            GeneralBrandFeatureMessages.InvalidRowVersion,
            ErrorType.Validation));

    private static Result<GeneralBrandResponse> ConcurrencyConflict() =>
        Result<GeneralBrandResponse>.Fail(new Error(
            "GeneralBrand.ConcurrencyConflict",
            GeneralBrandFeatureMessages.ConcurrencyConflict,
            ErrorType.Conflict));
}
