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

    public static string ParentScopeMismatch =>
        Get("Service_Parent_ScopeMismatch", "The selected parent service has a different service scope.");

    public static string ParentOwnerMismatch =>
        Get("Service_Parent_OwnerMismatch", "The selected parent service belongs to a different owner branch.");

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

    public static string TechnicalAdminRequired =>
        Get("Service_TechnicalAdmin_Required", "Only a technical administrator can perform this operation.");

    public static string BranchAdminRequired =>
        Get("Service_BranchAdmin_Required", "Only a branch administrator can perform this operation.");

    public static string BranchAccessForbidden =>
        Get("Service_BranchAccess_Forbidden", "The current user cannot access this branch.");

    public static string ActiveBranchRequired =>
        Get("Service_ActiveBranch_Required", "An active branch is required for this operation.");

    public static string UnsupportedActorType =>
        Get("Service_UnsupportedActorType", "The current user type cannot perform this operation.");

    public static string GlobalServiceForbiddenForBranchAdmin =>
        Get("Service_GlobalService_ForbiddenForBranchAdmin", "Branch administrators cannot modify global service definitions.");

    public static string ForeignBranchServiceForbidden =>
        Get("Service_ForeignBranchService_Forbidden", "Branch administrators can modify only service definitions owned by their active branch.");

    public static string BranchNotFound =>
        Get("Service_Branch_NotFound", "The branch could not be found.");

    public static string BranchInactive =>
        Get("Service_Branch_Inactive", "The branch is inactive.");

    public static string LeafIdsRequired =>
        Get("BranchServices_Assign_LeafIdsRequired", "At least one leaf service id is required.");

    public static string ServiceInactive =>
        Get("BranchServices_Assign_ServiceInactive", "The selected service is inactive.");

    public static string ServiceEffectivelyInactive =>
        Get("BranchServices_Assign_ServiceEffectivelyInactive", "The selected service is inactive because one of its parents is inactive or deleted.");

    public static string ServiceNotLeaf =>
        Get("BranchServices_Assign_ServiceNotLeaf", "Only leaf services can be assigned to a branch.");

    public static string InvalidHierarchy =>
        Get("BranchServices_Assign_InvalidHierarchy", "The service hierarchy is invalid.");

    public static string BranchServicesAssignSuccess =>
        Get("BranchServices_Assign_Success", "Services were assigned to the branch successfully.");

    public static string BranchServiceTreeRootRequired =>
        Get("BranchServiceTrees_Create_RootRequired", "A root service node is required.");

    public static string BranchServiceTreeParentCannotIssueTicket =>
        Get("BranchServiceTrees_Create_ParentCannotIssueTicket", "A parent service node cannot issue tickets.");

    public static string BranchServiceTreeTicketIssuableCannotHaveChildren =>
        Get("BranchServiceTrees_Create_TicketIssuableCannotHaveChildren", "A ticket-issuable service cannot have children.");

    public static string BranchServiceTreeDuplicateArabicName =>
        Get("BranchServiceTrees_Create_DuplicateArabicName", "A sibling service already uses this Arabic name.");

    public static string BranchServiceTreeDuplicateEnglishName =>
        Get("BranchServiceTrees_Create_DuplicateEnglishName", "A sibling service already uses this English name.");

    public static string BranchServiceTreePersistenceConflict =>
        Get("BranchServiceTrees_Create_PersistenceConflict", "The branch service tree could not be saved because of a data conflict.");

    public static string BranchServiceTreeCreateSuccess =>
        Get("BranchServiceTrees_Create_Success", "The branch service tree was created and assigned successfully.");

    public static string BranchServiceSubtreeCreateSuccess =>
        Get("Service_BranchSubtree_Create_Success", "The service subtree was created successfully.");

    public static string BranchServiceSubtreeRootRequired =>
        Get("Service_BranchSubtree_Root_Required", "A subtree root is required.");

    public static string BranchServiceSubtreeMaximumNodesExceeded =>
        Get("Service_BranchSubtree_MaximumNodesExceeded", "The subtree exceeds the maximum allowed number of service nodes.");

    public static string BranchServiceSubtreeParentNotFound =>
        Get("Service_BranchSubtree_Parent_NotFound", "The selected parent service could not be found.");

    public static string BranchServiceSubtreeParentDeleted =>
        Get("Service_BranchSubtree_Parent_Deleted", "The selected parent service is deleted.");

    public static string BranchServiceSubtreeParentInactive =>
        Get("Service_BranchSubtree_Parent_Inactive", "The selected parent service is inactive.");

    public static string BranchServiceSubtreeParentNotEffectivelyActive =>
        Get("Service_BranchSubtree_Parent_NotEffectivelyActive", "The selected parent service is not effectively active.");

    public static string BranchServiceSubtreeParentNotAssignedToBranch =>
        Get("Service_BranchSubtree_Parent_NotAssignedToBranch", "The selected parent service is not assigned to the target branch.");

    public static string BranchServiceSubtreeParentTicketIssuable =>
        Get("Service_BranchSubtree_Parent_TicketIssuable", "A ticket-issuable service cannot contain child services.");

    public static string BranchServiceSubtreeDuplicateArabicName =>
        Get("Service_BranchSubtree_DuplicateArabicName", "Another service under the selected parent already uses this Arabic name.");

    public static string BranchServiceSubtreeDuplicateEnglishName =>
        Get("Service_BranchSubtree_DuplicateEnglishName", "Another service under the selected parent already uses this English name.");

    public static string BranchServiceSubtreePersistenceConflict =>
        Get("Service_BranchSubtree_PersistenceConflict", "The service subtree could not be created because the data changed concurrently.");

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

    public static string BranchServicesUnassignAssignmentNotFound =>
    Get(
        "BranchServices_Unassign_AssignmentNotFound",
        "The selected leaf service is not assigned to the branch.");

    public static string BranchServicesUnassignServiceNotLeaf =>
        Get(
            "BranchServices_Unassign_ServiceNotLeaf",
            "Only leaf services can be unassigned from a branch.");

    public static string BranchServicesUnassignInvalidHierarchy =>
        Get(
            "BranchServices_Unassign_InvalidHierarchy",
            "The service hierarchy is invalid.");

    public static string BranchServicesUnassignPersistenceConflict =>
        Get(
            "BranchServices_Unassign_PersistenceConflict",
            "The service could not be unassigned because the data changed concurrently.");

    public static string BranchServicesUnassignSuccess =>
        Get(
            "BranchServices_Unassign_Success",
            "The service was unassigned and unused parent assignments were removed successfully.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}