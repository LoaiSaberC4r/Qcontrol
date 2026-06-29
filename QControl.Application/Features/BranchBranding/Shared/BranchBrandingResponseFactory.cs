using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchBranding.Shared;

internal static class BranchBrandingResponseFactory
{
    public static BranchBrandingResponse FromEntity(
        QControl.Domain.Entities.BranchBranding branding,
        string? message = null)
    {
        return new BranchBrandingResponse
        {
            BranchId = branding.BranchId,
            LogoUrl = BranchMediaUrlMapper.ToMediaUrl(branding.LogoPath),
            MainColor = branding.MainColor,
            SecondaryColor = branding.SecondaryColor,
            BackgroundColor = branding.BackgroundColor,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(branding.RowVersion),
            Message = message
        };
    }

    public static BranchBrandingResponse NotConfigured(int branchId)
    {
        return new BranchBrandingResponse
        {
            BranchId = branchId,
            IsConfigured = false
        };
    }
}
