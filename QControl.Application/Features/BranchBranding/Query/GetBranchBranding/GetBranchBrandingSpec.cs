using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchBranding.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;

internal sealed class GetBranchBrandingSpec
    : Specification<QControl.Domain.Entities.BranchBranding, BranchBrandingResponse>
{
    public GetBranchBrandingSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();

        Select(x => new BranchBrandingResponse
        {
            BranchId = x.BranchId,
            LogoUrl = x.LogoPath == null
                ? null
                : "/Media/" + x.LogoPath,
            MainColor = x.MainColor,
            SecondaryColor = x.SecondaryColor,
            BackgroundColor = x.BackgroundColor,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
