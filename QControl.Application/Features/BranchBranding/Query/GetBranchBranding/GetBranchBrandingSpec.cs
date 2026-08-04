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
            HeaderColor = x.HeaderColor,
            FooterColor = x.FooterColor,
            MainTextColor = x.MainTextColor,
            ShowLanguagePage = x.ShowLanguagePage,
            DefaultLanguageIsArabic = x.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = x.AlwaysRequireUserInput,
            ShowServiceNavigationPath = x.ShowServiceNavigationPath,
            AllowOperatorSelection = x.AllowOperatorSelection,
            AllowRequestMoreServices = x.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                x.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = x.LanguageButtonTextColor,
            LanguageButtonWidth = x.LanguageButtonWidth,
            LanguageButtonHeight = x.LanguageButtonHeight,
            LanguageButtonText = x.LanguageButtonText,
            ServiceButtonBackgroundColor =
                x.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = x.ServiceButtonTextColor,
            ServiceButtonWidth = x.ServiceButtonWidth,
            ServiceButtonHeight = x.ServiceButtonHeight,
            ServiceButtonSpace = x.ServiceButtonSpace,
            ServiceButtonFontSize = x.ServiceButtonFontSize,
            ServiceButtonText = x.ServiceButtonText,
            KeypadButtonBackgroundColor =
                x.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = x.KeypadButtonTextColor,
            KeypadButtonWidth = x.KeypadButtonWidth,
            KeypadButtonHeight = x.KeypadButtonHeight,
            KeypadButtonText = x.KeypadButtonText,
            FooterButtonBackgroundColor =
                x.FooterButtonBackgroundColor,
            FooterButtonTextColor = x.FooterButtonTextColor,
            FooterButtonWidth = x.FooterButtonWidth,
            FooterButtonHeight = x.FooterButtonHeight,
            FooterButtonText = x.FooterButtonText,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
