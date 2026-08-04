using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Abstraction.Presistence;

namespace Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;

internal sealed class CreateGeneralBrandCommandHandler
    : ICommandHandler<CreateGeneralBrandCommand, GeneralBrandResponse>
{
    private readonly IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandReadRepository;
    private readonly IWriteRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGeneralBrandCommandHandler(
        IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandReadRepository,
        IWriteRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _generalBrandReadRepository = generalBrandReadRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandReadRepository));
        _generalBrandWriteRepository = generalBrandWriteRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<GeneralBrandResponse>> Handle(
        CreateGeneralBrandCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GeneralBrandResponse>.Fail(new Error(
                "GeneralBrand.Create.Unauthenticated",
                GeneralBrandFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var alreadyConfigured = await _generalBrandReadRepository.AnyAsync(
            x => x.SingletonKey == 1,
            cancellationToken);

        if (alreadyConfigured)
        {
            return AlreadyConfigured();
        }

        var generalBrand = QControl.Domain.Entities.GeneralBrand.Create(
            BrandingLayoutSettingsFactory.FromInput(request),
            _currentUser.UserId.Value);

        await _generalBrandWriteRepository.AddAsync(
            generalBrand,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (GeneralBrandUniqueConstraintErrorMapper.TryMap(
                ex,
                out var error))
        {
            return Result<GeneralBrandResponse>.Fail(error);
        }

        return Result<GeneralBrandResponse>.Ok(
            GeneralBrandResponseFactory.FromEntity(
                generalBrand,
                GeneralBrandFeatureMessages.Created));
    }

    private static Result<GeneralBrandResponse> AlreadyConfigured() =>
        Result<GeneralBrandResponse>.Fail(new Error(
            "GeneralBrand.AlreadyConfigured",
            GeneralBrandFeatureMessages.AlreadyConfigured,
            ErrorType.Conflict));
}
