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
            HeaderColor = branding.HeaderColor,
            FooterColor = branding.FooterColor,
            MainTextColor = branding.MainTextColor,
            ShowLanguagePage = branding.ShowLanguagePage,
            DefaultLanguageIsArabic = branding.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = branding.AlwaysRequireUserInput,
            ShowServiceNavigationPath = branding.ShowServiceNavigationPath,
            AllowOperatorSelection = branding.AllowOperatorSelection,
            AllowRequestMoreServices = branding.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                branding.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = branding.LanguageButtonTextColor,
            LanguageButtonWidth = branding.LanguageButtonWidth,
            LanguageButtonHeight = branding.LanguageButtonHeight,
            LanguageButtonText = branding.LanguageButtonText,
            ServiceButtonBackgroundColor =
                branding.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = branding.ServiceButtonTextColor,
            ServiceButtonWidth = branding.ServiceButtonWidth,
            ServiceButtonHeight = branding.ServiceButtonHeight,
            ServiceButtonSpace = branding.ServiceButtonSpace,
            ServiceButtonFontSize = branding.ServiceButtonFontSize,
            ServiceButtonText = branding.ServiceButtonText,
            KeypadButtonBackgroundColor =
                branding.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = branding.KeypadButtonTextColor,
            KeypadButtonWidth = branding.KeypadButtonWidth,
            KeypadButtonHeight = branding.KeypadButtonHeight,
            KeypadButtonText = branding.KeypadButtonText,
            FooterButtonBackgroundColor =
                branding.FooterButtonBackgroundColor,
            FooterButtonTextColor = branding.FooterButtonTextColor,
            FooterButtonWidth = branding.FooterButtonWidth,
            FooterButtonHeight = branding.FooterButtonHeight,
            FooterButtonText = branding.FooterButtonText,
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
