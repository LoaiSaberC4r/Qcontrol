using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;

internal static class SegmentGlobalizationRequestMessages
{
    public static string AuthenticationRequired =>
        Get("SegmentGlobalizationRequests_AuthenticationRequired", "Authentication is required.");
    public static string TechnicalAdminRequired =>
        Get("SegmentGlobalizationRequests_TechnicalAdminRequired", "Only a technical administrator can perform this operation.");
    public static string BranchAccessForbidden =>
        Get("SegmentGlobalizationRequests_BranchAccessForbidden", "The current user cannot access requests for this branch.");
    public static string NotFound =>
        Get("SegmentGlobalizationRequests_NotFound", "The segment globalization request could not be found.");
    public static string NotPending =>
        Get("SegmentGlobalizationRequests_NotPending", "Only a pending request can be reviewed.");
    public static string SegmentNotFound =>
        Get("SegmentGlobalizationRequests_SegmentNotFound", "The related segment could not be found.");
    public static string ScopeChanged =>
        Get("SegmentGlobalizationRequests_ScopeChanged", "The segment is no longer branch scoped.");
    public static string OwnerChanged =>
        Get("SegmentGlobalizationRequests_OwnerChanged", "The segment owner no longer matches the request branch.");
    public static string DefaultCannotPromote =>
        Get("SegmentGlobalizationRequests_DefaultCannotPromote", "The default segment cannot be promoted.");
    public static string InvalidRowVersion =>
        Get("SegmentGlobalizationRequests_RowVersionInvalid", "The request row version is invalid.");
    public static string ConcurrencyConflict =>
        Get("SegmentGlobalizationRequests_ConcurrencyConflict", "The request changed concurrently.");
    public static string Approved =>
        Get("SegmentGlobalizationRequests_ApprovedSuccessfully", "The segment globalization request was approved.");
    public static string Rejected =>
        Get("SegmentGlobalizationRequests_RejectedSuccessfully", "The segment globalization request was rejected.");
    public static string PageNumberInvalid =>
        Get("SegmentGlobalizationRequests_PageNumberInvalid", "Page number must be at least 1.");
    public static string PageSizeInvalid =>
        Get("SegmentGlobalizationRequests_PageSizeInvalid", "Page size must be between 1 and 100.");
    public static string RejectionReasonMaxLength =>
        Get("SegmentGlobalizationRequests_RejectionReasonMaxLength", "The rejection reason must not exceed 500 characters.");

    private static string Get(string key, string fallback) =>
        ErrorMessage.ResourceManager.GetString(key, ErrorMessage.Culture)
        ?? fallback;
}
