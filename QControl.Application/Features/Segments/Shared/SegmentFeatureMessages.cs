using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Segments.Shared;

internal static class SegmentFeatureMessages
{
    public static string AuthenticationRequired =>
        Get("Segments_AuthenticationRequired", "Authentication is required.");
    public static string TechnicalAdminRequired =>
        Get("Segments_TechnicalAdminRequired", "Only a technical administrator can perform this operation.");
    public static string BranchAdminRequired =>
        Get("Segments_BranchAdminRequired", "Only a branch administrator can perform this operation.");
    public static string BranchAccessForbidden =>
        Get("Segments_BranchAccessForbidden", "The current user cannot access this branch.");
    public static string GlobalForbidden =>
        Get("Segments_GlobalSegmentForbiddenForBranchAdmin", "Branch administrators cannot update global segments.");
    public static string ForeignBranchForbidden =>
        Get("Segments_ForeignBranchSegmentForbidden", "The segment belongs to another branch.");
    public static string NotFound =>
        Get("Segments_NotFound", "The segment could not be found.");
    public static string BranchNotFound =>
        Get("Segments_BranchNotFound", "The branch could not be found.");
    public static string ArabicNameRequired =>
        Get("Segments_ArabicNameRequired", "The Arabic segment name is required.");
    public static string EnglishNameRequired =>
        Get("Segments_EnglishNameRequired", "The English segment name is required.");
    public static string NameMaxLength =>
        Get("Segments_NameMaxLength", "A segment name must not exceed 100 characters.");
    public static string PriorityNonNegative =>
        Get("Segments_InvalidPriority", "Priority must be greater than or equal to zero.");
    public static string PriorityExceedsCount =>
        Get("Segments_PriorityExceedsSegmentCount", "Priority cannot exceed the current segment count.");
    public static string DefaultPriorityCannotChange =>
        Get("Segments_DefaultPriorityCannotChange", "The default segment priority must remain zero.");
    public static string DefaultOnlyTechnicalAdmin =>
        Get("Segments_DefaultCanOnlyBeUpdatedByTechnicalAdmin", "Only a technical administrator can update the default segment names.");
    public static string InvalidRowVersion =>
        Get("Segments_RowVersionInvalid", "The segment row version is invalid.");
    public static string ConcurrencyConflict =>
        Get("Segments_ConcurrencyConflict", "The segment was changed by another operation.");
    public static string PendingRequestExists =>
        Get("SegmentGlobalizationRequests_AlreadyPending", "A pending globalization request already exists for this segment.");
    public static string CreateSuccess =>
        Get("Segments_CreatedSuccessfully", "The segment was created successfully.");
    public static string UpdateSuccess =>
        Get("Segments_UpdatedSuccessfully", "The segment was updated successfully.");
    public static string PageNumberInvalid =>
        Get("Segments_PageNumberInvalid", "Page number must be at least 1.");
    public static string PageSizeInvalid =>
        Get("Segments_PageSizeInvalid", "Page size must be between 1 and 100.");

    private static string Get(string key, string fallback) =>
        ErrorMessage.ResourceManager.GetString(key, ErrorMessage.Culture)
        ?? fallback;
}
