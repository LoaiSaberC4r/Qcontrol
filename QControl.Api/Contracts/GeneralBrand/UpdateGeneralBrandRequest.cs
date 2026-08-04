namespace Qcontrol.Api.Contracts.GeneralBrand;

public sealed class UpdateGeneralBrandRequest : GeneralBrandLayoutRequest
{
    public string RowVersion { get; init; } = string.Empty;
}
