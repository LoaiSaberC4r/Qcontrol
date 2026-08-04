using BuildingBlock.Domain.Specification;

namespace Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;

internal sealed class GetGeneralBrandForUpdateSpec
    : Specification<QControl.Domain.Entities.GeneralBrand>
{
    public GetGeneralBrandForUpdateSpec()
    {
        AddCriteria(x => x.SingletonKey == 1);
        UseTracking();
    }
}
