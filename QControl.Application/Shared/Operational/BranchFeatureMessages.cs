using Qcontrol.Domain.Resources;

namespace QControl.Application.Shared.Operational;

internal static class BranchFeatureMessages
{
    public static string BrandingSaved =>
        Get(nameof(BrandingSaved), "BranchBranding_Save_Success");

    public static string ThemeSaved =>
        Get(nameof(ThemeSaved), "BranchBranding_Theme_Save_Success");

    public static string LogoUploaded =>
        Get(nameof(LogoUploaded), "BranchBranding_Logo_Upload_Success");

    public static string InvalidColor =>
        Get(nameof(InvalidColor), "BranchBranding_InvalidColor");

    public static string InvalidDimension =>
        Get(nameof(InvalidDimension), "BranchBranding_InvalidDimension");

    public static string InvalidSpacing =>
        Get(nameof(InvalidSpacing), "BranchBranding_InvalidSpacing");

    public static string ButtonTextTooLong =>
        Get(nameof(ButtonTextTooLong), "BranchBranding_ButtonText_MaxLength");

    public static string LogoRequired =>
        Get(nameof(LogoRequired), "BranchBranding_Logo_Required");

    public static string BrandingInvalidImageType =>
        Get(nameof(BrandingInvalidImageType), "BranchBranding_InvalidImageType");

    public static string BrandingMediaSaveFailed =>
        Get(nameof(BrandingMediaSaveFailed), "BranchBranding_MediaSaveFailed");

    public static string AdvertisementImagesRequired =>
        Get(nameof(AdvertisementImagesRequired), "BranchAdvertisement_Images_Required");

    public static string AdvertisementInvalidImageType =>
        Get(nameof(AdvertisementInvalidImageType), "BranchAdvertisement_InvalidImageType");

    public static string AdvertisementLimitExceeded =>
        Get(nameof(AdvertisementLimitExceeded), "BranchAdvertisement_LimitExceeded");

    public static string AdvertisementNotFound =>
        Get(nameof(AdvertisementNotFound), "BranchAdvertisement_NotFound");

    public static string AdvertisementOwnershipMismatch =>
        Get(nameof(AdvertisementOwnershipMismatch), "BranchAdvertisement_DoesNotBelongToBranch");

    public static string InvalidDisplayOrder =>
        Get(nameof(InvalidDisplayOrder), "BranchAdvertisement_InvalidDisplayOrder");

    public static string DuplicateDisplayOrder =>
        Get(nameof(DuplicateDisplayOrder), "BranchAdvertisement_DuplicateDisplayOrder");

    public static string IncompleteReorder =>
        Get(nameof(IncompleteReorder), "BranchAdvertisement_IncompleteReorder");

    public static string AlreadyActive =>
        Get(nameof(AlreadyActive), "BranchAdvertisement_AlreadyActive");

    public static string AlreadyInactive =>
        Get(nameof(AlreadyInactive), "BranchAdvertisement_AlreadyInactive");

    public static string AdvertisementDeactivated =>
        Get(nameof(AdvertisementDeactivated), "BranchAdvertisement_Deactivate_Success");

    public static string AdvertisementReactivated =>
        Get(nameof(AdvertisementReactivated), "BranchAdvertisement_Reactivate_Success");

    public static string AdvertisementDeleted =>
        Get(nameof(AdvertisementDeleted), "BranchAdvertisement_Delete_Success");

    public static string AdvertisementMediaSaveFailed =>
        Get(nameof(AdvertisementMediaSaveFailed), "BranchAdvertisement_MediaSaveFailed");

    public static string AdvertisementOrderConflict =>
        Get(nameof(AdvertisementOrderConflict), "BranchAdvertisement_OrderConflict");

    public static string AdvertisementIdRequired =>
        Get(nameof(AdvertisementIdRequired), "BranchAdvertisement_Id_Required");

    private static string Get(string fallback, string resourceName) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
