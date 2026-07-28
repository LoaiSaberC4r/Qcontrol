using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

internal static class BranchServiceSegmentMessages
{
    public static string AuthenticationRequired =>
        Get("BranchServiceSegments_AuthenticationRequired", "Authentication is required.");
    public static string BranchNotFound =>
        Get("BranchServiceSegments_BranchNotFound", "The branch could not be found.");
    public static string BranchAccessForbidden =>
        Get("BranchServiceSegments_BranchAccessForbidden", "The current user cannot access this branch.");
    public static string ServiceNotFound =>
        Get("BranchServiceSegments_ServiceNotFound", "The service could not be found.");
    public static string ServiceNotAssigned =>
        Get("BranchServiceSegments_ServiceNotAssigned", "The service is not assigned to this branch.");
    public static string ServiceNotLeaf =>
        Get("BranchServiceSegments_ServiceNotLeaf", "The service must be a leaf service.");
    public static string ServiceNotTicketIssuable =>
        Get("BranchServiceSegments_ServiceNotTicketIssuable", "The service cannot issue tickets.");
    public static string ServiceUnavailable =>
        Get("BranchServiceSegments_ServiceUnavailable", "The service is not effectively available.");
    public static string InvalidServiceRange =>
        Get("BranchServiceSegments_InvalidServiceRange", "The service ticket range is invalid.");
    public static string SegmentNotFound =>
        Get("BranchServiceSegments_SegmentNotFound", "One or more segments could not be found.");
    public static string SegmentNotVisible =>
        Get("BranchServiceSegments_SegmentNotVisible", "One or more segments are not available to this branch.");
    public static string SegmentAlreadyAssigned =>
        Get("BranchServiceSegments_SegmentAlreadyAssigned", "One or more segments are already assigned.");
    public static string DuplicateSegmentIds =>
        Get("BranchServiceSegments_DuplicateSegmentIds", "The request contains duplicate segment ids.");
    public static string ItemsRequired =>
        Get("BranchServiceSegments_ItemsRequired", "At least one segment assignment is required.");
    public static string DefaultCannotBeAssigned =>
        Get("BranchServiceSegments_DefaultCannotBeAssigned", "The default segment is assigned automatically.");
    public static string DefaultCannotBeUnassigned =>
        Get("BranchServiceSegments_DefaultCannotBeUnassigned", "The default segment cannot be unassigned.");
    public static string DefaultQuotaSystemCalculated =>
        Get("BranchServiceSegments_DefaultQuotaIsSystemCalculated", "The default segment quota is calculated by the system.");
    public static string QuotaNonNegative =>
        Get("BranchServiceSegments_QuotaNonNegative", "Quota must be greater than or equal to zero.");
    public static string QuotaExceedsCapacity =>
        Get("BranchServiceSegments_QuotaExceedsServiceCapacity", "The total segment quota exceeds the service capacity.");
    public static string RelationshipNotFound =>
        Get("BranchServiceSegments_RelationshipNotFound", "The segment assignment could not be found.");
    public static string InvalidRowVersion =>
        Get("BranchServiceSegments_RowVersionInvalid", "The assignment row version is invalid.");
    public static string ConcurrencyConflict =>
        Get("BranchServiceSegments_ConcurrencyConflict", "The segment assignments changed concurrently.");
    public static string AssignSuccess =>
        Get("BranchServiceSegments_AssignedSuccessfully", "Segments were assigned successfully.");
    public static string QuotaUpdateSuccess =>
        Get("BranchServiceSegments_QuotaUpdatedSuccessfully", "The segment quota was updated successfully.");
    public static string UnassignSuccess =>
        Get("BranchServiceSegments_UnassignedSuccessfully", "The segment was unassigned successfully.");
    public static string PageNumberInvalid =>
        Get("BranchServiceSegments_PageNumberInvalid", "Page number must be at least 1.");
    public static string PageSizeInvalid =>
        Get("BranchServiceSegments_PageSizeInvalid", "Page size must be between 1 and 100.");

    private static string Get(string key, string fallback) =>
        ErrorMessage.ResourceManager.GetString(key, ErrorMessage.Culture)
        ?? fallback;
}
