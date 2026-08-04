using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Query.GetGeneralBrand;

internal sealed class GetGeneralBrandSpec
    : Specification<
        QControl.Domain.Entities.GeneralBrand,
        GeneralBrandResponse>
{
    public GetGeneralBrandSpec()
    {
        UseNoTracking();

        Select(x => new GeneralBrandResponse
        {
            Id = x.Id,
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
