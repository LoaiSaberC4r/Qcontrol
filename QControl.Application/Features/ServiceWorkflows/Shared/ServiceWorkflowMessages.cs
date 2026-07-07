using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowMessages
{
    public static string AuthenticationRequired =>
        Get("ServiceWorkflow_Authentication_Required", "Authentication is required.");

    public static string IdRequired =>
        Get("ServiceWorkflow_Id_Required", "A valid workflow id is required.");

    public static string NotFound =>
        Get("ServiceWorkflow_NotFound", "The service workflow could not be found.");

    public static string ArabicNameRequired =>
        Get("ServiceWorkflow_ArabicName_Required", "The Arabic workflow name is required.");

    public static string ArabicNameMaxLength =>
        Get("ServiceWorkflow_ArabicName_MaxLength", "The Arabic workflow name must not exceed 100 characters.");

    public static string EnglishNameRequired =>
        Get("ServiceWorkflow_EnglishName_Required", "The English workflow name is required.");

    public static string EnglishNameMaxLength =>
        Get("ServiceWorkflow_EnglishName_MaxLength", "The English workflow name must not exceed 100 characters.");

    public static string NameAlreadyExists =>
        Get("ServiceWorkflow_Name_AlreadyExists", "Another workflow already uses this name.");

    public static string StepsRequired =>
        Get("ServiceWorkflow_Steps_Required", "Workflow steps are required.");

    public static string MinimumStepsRequired =>
        Get("ServiceWorkflow_MinimumSteps_Required", "A workflow must contain at least two steps.");

    public static string StepOrderInvalid =>
        Get("ServiceWorkflow_StepOrder_Invalid", "Step order must be greater than zero.");

    public static string StepOrderDuplicate =>
        Get("ServiceWorkflow_StepOrder_Duplicate", "Step order must be unique within the workflow.");

    public static string StepOrderNotSequential =>
        Get("ServiceWorkflow_StepOrder_NotSequential", "Step order must be sequential starting from 1.");

    public static string ServiceIdRequired =>
        Get("ServiceWorkflow_ServiceId_Required", "A valid service id is required.");

    public static string ServiceNotFound =>
        Get("ServiceWorkflow_Service_NotFound", "The selected service could not be found.");

    public static string ServiceDeleted =>
        Get("ServiceWorkflow_Service_Deleted", "The selected service is deleted.");

    public static string ServiceInactive =>
        Get("ServiceWorkflow_Service_Inactive", "The selected service is inactive.");

    public static string ServiceNotTicketIssuable =>
        Get("ServiceWorkflow_Service_NotTicketIssuable", "The selected service cannot be used in a workflow because it does not issue tickets.");

    public static string ServiceNotEffectivelyActive =>
        Get("ServiceWorkflow_Service_NotEffectivelyActive", "The selected service cannot be used in a workflow because it is not effectively active.");

    public static string AlreadyInactive =>
        Get("ServiceWorkflow_AlreadyInactive", "The service workflow is already inactive.");

    public static string AlreadyActive =>
        Get("ServiceWorkflow_AlreadyActive", "The service workflow is already active.");

    public static string DeactivateSuccess =>
        Get("ServiceWorkflow_Deactivate_Success", "Service workflow deactivated successfully.");

    public static string ReactivateSuccess =>
        Get("ServiceWorkflow_Reactivate_Success", "Service workflow reactivated successfully.");

    public static string CreateSuccess =>
        Get("ServiceWorkflow_Create_Success", "Service workflow created successfully.");

    public static string UpdateSuccess =>
        Get("ServiceWorkflow_Update_Success", "Service workflow updated successfully.");

    public static string PageNumberInvalid =>
        Get("ServiceWorkflow_Pagination_PageNumber_Invalid", "Page number must be at least 1.");

    public static string PageSizeInvalid =>
        Get("ServiceWorkflow_Pagination_PageSize_Invalid", "Page size must be at least 1.");

    public static string PageSizeMax =>
        Get("ServiceWorkflow_Pagination_PageSize_Max", "Page size must not exceed 100.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
