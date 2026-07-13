using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

internal static class ServiceGlobalizationRequestMessages
{
    public static string RequestIdRequired =>
        Get("ServiceGlobalizationRequests_RequestId_Required", "A valid request id is required.");

    public static string RequestNotFound =>
        Get("ServiceGlobalizationRequests_NotFound", "The service globalization request could not be found.");

    public static string AlreadyApproved =>
        Get("ServiceGlobalizationRequests_AlreadyApproved", "The request has already been approved.");

    public static string AlreadyRejected =>
        Get("ServiceGlobalizationRequests_AlreadyRejected", "The request has already been rejected.");

    public static string NotPending =>
        Get("ServiceGlobalizationRequests_NotPending", "Only pending requests can be reviewed.");

    public static string CreatedSuccessfully =>
        Get("ServiceGlobalizationRequests_CreatedSuccessfully", "The service globalization request was created successfully.");

    public static string ApprovedSuccessfully =>
        Get("ServiceGlobalizationRequests_ApprovedSuccessfully", "The service globalization request was approved successfully.");

    public static string RejectedSuccessfully =>
        Get("ServiceGlobalizationRequests_RejectedSuccessfully", "The service globalization request was rejected.");

    public static string RejectionReasonMaxLength =>
        Get("ServiceGlobalizationRequests_RejectionReason_MaxLength", "The rejection reason must not exceed 1000 characters.");

    public static string InvalidRowVersion =>
        Get("ServiceGlobalizationRequests_RowVersion_Invalid", "The request row version is invalid.");

    public static string ConcurrencyConflict =>
        Get("ServiceGlobalizationRequests_ConcurrencyConflict", "The request was changed by another operation.");

    public static string TechnicalAdminRequired =>
        Get("ServiceGlobalizationRequests_TechnicalAdmin_Required", "Only a technical administrator can review service globalization requests.");

    public static string BranchRequestAccessForbidden =>
        Get("ServiceGlobalizationRequests_BranchAccess_Forbidden", "The current user cannot access requests for this branch.");

    public static string SubmittedServiceNotFound =>
        Get("ServiceGlobalizationRequests_SubmittedService_NotFound", "A submitted service could not be found.");

    public static string SubmittedServiceDeleted =>
        Get("ServiceGlobalizationRequests_SubmittedService_Deleted", "A submitted service is deleted.");

    public static string SubmittedServiceScopeChanged =>
        Get("ServiceGlobalizationRequests_SubmittedService_ScopeChanged", "A submitted service is no longer branch scoped.");

    public static string SubmittedServiceOwnerChanged =>
        Get("ServiceGlobalizationRequests_SubmittedService_OwnerChanged", "A submitted service owner changed.");

    public static string SubmittedHierarchyInvalid =>
        Get("ServiceGlobalizationRequests_SubmittedHierarchy_Invalid", "The submitted service hierarchy is no longer valid.");

    public static string GlobalParentRequired =>
        Get("ServiceGlobalizationRequests_GlobalParent_Required", "A global parent service is required.");

    public static string GlobalParentChanged =>
        Get("ServiceGlobalizationRequests_GlobalParent_Changed", "The global parent changed.");

    public static string GlobalParentDeleted =>
        Get("ServiceGlobalizationRequests_GlobalParent_Deleted", "The global parent service is deleted.");

    public static string LeafUnderGlobalParentCannotContainChildren =>
        Get("ServiceGlobalizationRequests_LeafUnderGlobalParent_CannotContainChildren", "A leaf under a global parent cannot contain child services.");

    public static string DuplicateGlobalArabicName =>
        Get("ServiceGlobalizationRequests_DuplicateGlobalArabicName", "A global sibling already uses this Arabic name.");

    public static string DuplicateGlobalEnglishName =>
        Get("ServiceGlobalizationRequests_DuplicateGlobalEnglishName", "A global sibling already uses this English name.");

    public static string PendingRequestAlreadyExists =>
        Get("ServiceGlobalizationRequests_PendingRequestAlreadyExists", "A pending globalization request already exists for this service.");

    public static string RequestItemMismatch =>
        Get("ServiceGlobalizationRequests_ItemMismatch", "The request items no longer match the submitted services.");

    public static string PageNumberInvalid =>
        Get("ServiceGlobalizationRequests_PageNumber_Invalid", "Page number must be at least 1.");

    public static string PageSizeInvalid =>
        Get("ServiceGlobalizationRequests_PageSize_Invalid", "Page size must be at least 1.");

    public static string PageSizeMax =>
        Get("ServiceGlobalizationRequests_PageSize_Max", "Page size must not exceed 100.");

    public static string SearchTextMaxLength =>
        Get("ServiceGlobalizationRequests_SearchText_MaxLength", "Search text must not exceed 200 characters.");

    public static string AuthenticationRequired =>
        Get("ServiceGlobalizationRequests_Authentication_Required", "Authentication is required.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
