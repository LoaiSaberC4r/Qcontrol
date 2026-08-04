using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;

public sealed record CreateGeneralBrandCommand
    : GeneralBrandLayoutInput,
      ICommand<GeneralBrandResponse>,
      ICacheInvalidator
{
    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.GeneralBrand,
        OperationalCacheTags.GeneralBrandSingleton
    };
}
