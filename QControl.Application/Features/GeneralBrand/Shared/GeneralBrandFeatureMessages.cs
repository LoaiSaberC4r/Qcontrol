using Qcontrol.Domain.Resources;
using Qcontrol.Application.Features.BranchBranding.Shared;

namespace Qcontrol.Application.Features.GeneralBrand.Shared;

internal static class GeneralBrandFeatureMessages
{
    public static string Created => Get(
        nameof(Created),
        "GeneralBrand_Created");

    public static string Updated => Get(
        nameof(Updated),
        "GeneralBrand_Updated");

    public static string AlreadyConfigured => Get(
        nameof(AlreadyConfigured),
        "GeneralBrand_AlreadyConfigured");

    public static string NotConfigured => Get(
        nameof(NotConfigured),
        "GeneralBrand_NotConfigured");

    public static string InvalidColor => Get(
        nameof(InvalidColor),
        "GeneralBrand_InvalidColor");

    public static string InvalidDimension => Get(
        nameof(InvalidDimension),
        "GeneralBrand_InvalidDimension");

    public static string InvalidSpacing => Get(
        nameof(InvalidSpacing),
        "GeneralBrand_InvalidSpacing");

    public static string ButtonTextTooLong => Get(
        nameof(ButtonTextTooLong),
        "GeneralBrand_ButtonText_MaxLength");

    public static string InvalidRowVersion => Get(
        nameof(InvalidRowVersion),
        "GeneralBrand_InvalidRowVersion");

    public static string ConcurrencyConflict => Get(
        nameof(ConcurrencyConflict),
        "GeneralBrand_ConcurrencyConflict");

    public static string AuthenticationRequired => Get(
        nameof(AuthenticationRequired),
        "GeneralBrand_AuthenticationRequired");

    public static BrandingLayoutValidationMessages ValidationMessages() =>
        new(
            InvalidColor,
            InvalidDimension,
            InvalidSpacing,
            ButtonTextTooLong);

    private static string Get(string fallback, string resourceName) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
