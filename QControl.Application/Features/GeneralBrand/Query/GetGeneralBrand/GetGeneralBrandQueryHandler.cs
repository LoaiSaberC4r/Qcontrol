using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Abstraction.Presistence;

namespace Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;

internal sealed class GetGeneralBrandQueryHandler
    : IQueryHandler<GetGeneralBrandQuery, GeneralBrandResponse>
{
    private readonly IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetGeneralBrandQueryHandler(
        IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandReadRepository,
        ICurrentUser currentUser)
    {
        _generalBrandReadRepository = generalBrandReadRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GeneralBrandResponse>> Handle(
        GetGeneralBrandQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GeneralBrandResponse>.Fail(new Error(
                "GeneralBrand.Get.Unauthenticated",
                GeneralBrandFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var generalBrand = await _generalBrandReadRepository
            .FirstOrDefaultAsync(
                new GetGeneralBrandSpec(),
                cancellationToken);

        return Result<GeneralBrandResponse>.Ok(
            generalBrand ?? GeneralBrandResponseFactory.NotConfigured());
    }
}
