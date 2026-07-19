using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

internal static class ServiceScheduleMessages
{
    public static string AuthenticationRequired =>
        Get("ServiceSchedule_Authentication_Required", "Authentication is required.");

    public static string BranchIdRequired =>
        Get("ServiceSchedule_BranchId_Required", "A valid branch id is required.");

    public static string LeafServiceIdRequired =>
        Get("ServiceSchedule_LeafServiceId_Required", "A valid leaf service id is required.");

    public static string BranchNotFound =>
        Get("ServiceSchedule_Branch_NotFound", "The selected branch could not be found.");

    public static string BranchInactive =>
        Get("ServiceSchedule_Branch_Inactive", "The selected branch is inactive.");

    public static string ServiceNotFound =>
        Get("ServiceSchedule_Service_NotFound", "The selected service could not be found.");

    public static string ServiceNotAssignedToBranch =>
        Get("ServiceSchedule_Service_NotAssignedToBranch", "The selected service is not assigned to this branch.");

    public static string ServiceIsNotLeaf =>
        Get("ServiceSchedule_Service_IsNotLeaf", "Only ticket-issuing leaf services can have a service schedule.");

    public static string ServiceNotTicketIssuable =>
        Get("ServiceSchedule_Service_NotTicketIssuable", "Only ticket-issuing leaf services can have a service schedule.");

    public static string ServiceDeleted =>
        Get("ServiceSchedule_Service_Deleted", "The selected service is deleted.");

    public static string ServiceInactive =>
        Get("ServiceSchedule_Service_Inactive", "The selected service is inactive.");

    public static string ServiceNotEffectivelyActive =>
        Get("ServiceSchedule_Service_NotEffectivelyActive", "The selected service is not effectively active.");

    public static string ScheduleNotFound =>
        Get("ServiceSchedule_NotFound", "The service schedule could not be found.");

    public static string ScheduleAlreadyExists =>
        Get("ServiceSchedule_AlreadyExists", "A service schedule already exists for this service in the selected branch.");

    public static string InvalidTimeRange =>
        Get("ServiceSchedule_TimeRange_Invalid", "The start time must be earlier than the end time.");

    public static string WorkDaysRequired =>
        Get("ServiceSchedule_WorkDays_Required", "At least one working day is required.");

    public static string DuplicateWorkDay =>
        Get("ServiceSchedule_WorkDay_Duplicate", "The same working day cannot be selected more than once.");

    public static string InvalidWorkDay =>
        Get("ServiceSchedule_WorkDay_Invalid", "The supplied working day is invalid.");

    public static string SlotCodeRequired =>
        Get("ServiceSchedule_SlotCode_Required", "A slot code is required for this schedule.");

    public static string SlotCodeInvalid =>
        Get("ServiceSchedule_SlotCode_Invalid", "The slot code must not be blank.");

    public static string SlotCodeMaxLength =>
        Get("ServiceSchedule_SlotCode_MaxLength", "The slot code must not exceed 100 characters.");

    public static string SlotCodeAlreadyExists =>
        Get("ServiceSchedule_SlotCode_AlreadyExists", "The slot code is already used by another service schedule in this branch.");

    public static string CreateSuccess =>
        Get("ServiceSchedule_Create_Success", "The service schedule was created successfully.");

    public static string UpdateSuccess =>
        Get("ServiceSchedule_Update_Success", "The service schedule was updated successfully.");

    public static string ConcurrencyConflict =>
        Get("ServiceSchedule_Concurrency_Conflict", "The service schedule was changed by another operation.");

    public static string BranchPermanentDeleteServiceSchedulesExist =>
        Get("Branch_PermanentDelete_ServiceSchedulesExist", "The branch cannot be permanently deleted because service schedules exist.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(resourceName, ErrorMessage.Culture)
        ?? fallback;
}
