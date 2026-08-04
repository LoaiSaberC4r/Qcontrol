using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.GeneralBrand.Shared;

namespace Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;

public sealed record GetGeneralBrandQuery
    : IQuery<GeneralBrandResponse>;
