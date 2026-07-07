using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceFeatureMessages
{
    public static string AuthenticationRequired =>
        Get("Service_Authentication_Required", "Authentication is required.");

    public static string IdRequired =>
        Get("Service_Id_Required", "A valid service id is required.");

    public static string ParentIdRequired =>
        Get("Service_ParentId_Required", "A valid parent service id is required.");

    public static string NotFound =>
        Get("Service_NotFound", "The service could not be found.");

    public static string Deleted =>
        Get("Service_Deleted", "The service is deleted.");

    public static string AlreadyDeleted =>
        Get("Service_AlreadyDeleted", "The service is already deleted.");

    public static string NotDeleted =>
        Get("Service_NotDeleted", "The service is not deleted.");

    public static string ParentNotFound =>
        Get("Service_Parent_NotFound", "The selected parent service could not be found.");

    public static string ParentDeleted =>
        Get("Service_Parent_Deleted", "The selected parent service is deleted.");

    public static string ParentInactive =>
        Get("Service_Parent_Inactive", "The selected parent service is inactive.");

    public static string ParentTicketIssuable =>
        Get("Service_Parent_TicketIssuable", "A ticket-issuable service cannot be selected as a parent.");

    public static string ParentHasHistoricalTickets =>
        Get("Service_Parent_HasHistoricalTickets", "The selected parent service has historical tickets.");

    public static string CircularHierarchy =>
        Get("Service_CircularHierarchy", "The selected parent would create a circular service hierarchy.");

    public static string HasChildrenCannotBeTicketIssuable =>
        Get("Service_HasChildrenCannotBeTicketIssuable", "A service with children cannot issue tickets.");

    public static string HistoricalTicketsPreventCategory =>
        Get("Service_HistoricalTicketsPreventCategory", "A service that has historical tickets cannot become a parent/category service.");

    public static string DuplicateArabicName =>
        Get("Service_DuplicateArabicName", "Another service under the same parent already uses this Arabic name.");

    public static string DuplicateEnglishName =>
        Get("Service_DuplicateEnglishName", "Another service under the same parent already uses this English name.");

    public static string CreateSuccess =>
        Get("Service_Create_Success", "Service created successfully.");

    public static string UpdateSuccess =>
        Get("Service_Update_Success", "Service updated successfully.");

    public static string DeleteSuccess =>
        Get("Service_Delete_Success", "Service deleted successfully.");

    public static string RestoreSuccess =>
        Get("Service_Restore_Success", "Service restored successfully.");

    public static string LogoUploaded =>
        Get("Service_Logo_Upload_Success", "Service logo uploaded successfully.");

    public static string InvalidImageType =>
        Get("Service_Logo_InvalidImageType", "Only PNG, JPG, JPEG, and WEBP images are supported.");

    public static string MediaSaveFailed =>
        Get("Service_Logo_MediaSaveFailed", "The service logo could not be saved. Please try again.");

    public static string IconUploaded =>
        Get("Service_Icon_Upload_Success", "Service icon uploaded successfully.");

    public static string AdsUploaded =>
        Get("Service_Ads_Upload_Success", "Service advertisement images uploaded successfully.");

    public static string ImageDeleted =>
        Get("Service_Image_Delete_Success", "Service image deleted successfully.");

    public static string ImageRequired =>
        Get("Service_Image_Required", "A service image is required.");

    public static string ImagesRequired =>
        Get("Service_Images_Required", "At least one service image is required.");

    public static string ImageIdRequired =>
        Get("Service_Image_Id_Required", "A valid service image id is required.");

    public static string ImageNotFound =>
        Get("Service_Image_NotFound", "The service image could not be found.");

    public static string ImageConflict =>
        Get("Service_Image_Conflict", "The service image could not be saved because another image changed at the same time.");

    public static string InvalidImageTypeFilter =>
        Get("Service_ImageType_Invalid", "The service image type is invalid.");

    public static string ArabicNameRequired =>
        Get("Service_ArabicName_Required", "The Arabic service name is required.");

    public static string ArabicNameMaxLength =>
        Get("Service_ArabicName_MaxLength", "The Arabic service name must not exceed 100 characters.");

    public static string EnglishNameRequired =>
        Get("Service_EnglishName_Required", "The English service name is required.");

    public static string EnglishNameMaxLength =>
        Get("Service_EnglishName_MaxLength", "The English service name must not exceed 100 characters.");

    public static string ArabicUserMessageMaxLength =>
        Get("Service_ArabicUserMessage_MaxLength", "The Arabic user message must not exceed 500 characters.");

    public static string EnglishUserMessageMaxLength =>
        Get("Service_EnglishUserMessage_MaxLength", "The English user message must not exceed 500 characters.");

    public static string IsTicketIssuableRequired =>
        Get("Service_IsTicketIssuable_Required", "The ticket-issuable flag is required.");

    public static string RangePrefixRequired =>
        Get("Service_RangePrefix_Required", "The range prefix is required.");

    public static string RangePrefixMaxLength =>
        Get("Service_RangePrefix_MaxLength", "The range prefix must not exceed 10 characters.");

    public static string TicketSettingsRequired =>
        Get("Service_TicketSettings_Required", "Ticket settings are required when the service issues tickets.");

    public static string RangeStartRequired =>
        Get("Service_RangeStart_Required", "Range start number is required when the service issues tickets.");

    public static string RangeStartNonNegative =>
        Get("Service_RangeStart_NonNegative", "Range start number must be greater than or equal to zero.");

    public static string RangeEndRequired =>
        Get("Service_RangeEnd_Required", "Range end number is required when the service issues tickets.");

    public static string RangeEndGreaterOrEqualStart =>
        Get("Service_RangeEnd_GreaterOrEqualStart", "Range end number must be greater than or equal to range start number.");

    public static string OrderNoNonNegative =>
        Get("Service_OrderNo_NonNegative", "Order number must be greater than or equal to zero.");

    public static string PriorityNonNegative =>
        Get("Service_Priority_NonNegative", "Priority must be greater than or equal to zero.");

    public static string WaitingDurationRequired =>
        Get("Service_WaitingDuration_Required", "Waiting duration is required when the service issues tickets.");

    public static string WaitingDurationNonNegative =>
        Get("Service_WaitingDuration_NonNegative", "Waiting duration must be greater than or equal to zero.");

    public static string NoOfTicketCopiesRequired =>
        Get("Service_NoOfTicketCopies_Required", "Number of ticket copies is required when the service issues tickets.");

    public static string NoOfTicketCopiesPositive =>
        Get("Service_NoOfTicketCopies_Positive", "Number of ticket copies must be greater than zero.");

    public static string PageNumberInvalid =>
        Get("Service_Pagination_PageNumber_Invalid", "Page number must be at least 1.");

    public static string PageSizeInvalid =>
        Get("Service_Pagination_PageSize_Invalid", "Page size must be at least 1.");

    public static string PageSizeMax =>
        Get("Service_Pagination_PageSize_Max", "Page size must not exceed 100.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
