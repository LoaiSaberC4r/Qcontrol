using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;

public sealed record UpdateGeneralBrandCommand
    : GeneralBrandLayoutInput,
      ICommand<GeneralBrandResponse>,
      ICacheInvalidator
{
    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.GeneralBrand,
        OperationalCacheTags.GeneralBrandSingleton
    };
}
