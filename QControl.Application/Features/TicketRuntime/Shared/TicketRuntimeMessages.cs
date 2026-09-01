using System.Globalization;
using Qcontrol.Domain.Resources;

namespace QControl.Application.Features.TicketRuntime.Shared;

public static class TicketRuntimeMessages
{
    private static string Get(string key, string english) =>
        ErrorMessage.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? english;

    public static string AuthenticationRequired => Get("TicketRuntime_AuthenticationRequired", "Authentication is required.");
    public static string TicketNotFound => Get("TicketRuntime_TicketNotFound", "Ticket was not found.");
    public static string ReservationNotFound => Get("TicketRuntime_ReservationNotFound", "Reservation was not found.");
    public static string WrongBranch => Get("TicketRuntime_WrongBranch", "The record does not belong to this branch.");
    public static string InvalidTransition => Get("TicketRuntime_InvalidTransition", "The requested status transition is not allowed.");
    public static string TicketCannotCancelAfterStart => Get("TicketRuntime_TicketCannotCancelAfterStart", "A ticket cannot be cancelled after service starts.");
    public static string TicketMustBeCalled => Get("TicketRuntime_TicketMustBeCalled", "The ticket must be Called before service can start.");
    public static string TicketMustBeInProgress => Get("TicketRuntime_TicketMustBeInProgress", "The ticket must be InProgress for this operation.");
    public static string TicketMustBeNoShow => Get("TicketRuntime_TicketMustBeNoShow", "The ticket must be NoShow to return it to Waiting.");
    public static string WorkflowTransferForbidden => Get("TicketRuntime_WorkflowTransferForbidden", "A workflow-bound ticket cannot be manually transferred.");
    public static string ServiceUnavailable => Get("TicketRuntime_ServiceUnavailable", "The service is not currently available in this branch.");
    public static string WorkflowServiceUnavailable => Get("TicketRuntime_WorkflowServiceUnavailable", "The next workflow service is not currently available.");
    public static string NumberRangeExhausted => Get("TicketRuntime_NumberRangeExhausted", "The service ticket number range is exhausted for this business day.");
    public static string QuotaExceeded => Get("TicketRuntime_QuotaExceeded", "The segment daily quota has been reached.");
    public static string ConcurrencyConflict => Get("TicketRuntime_ConcurrencyConflict", "The record changed during this operation. Refresh and retry.");
    public static string AlreadyConverted => Get("TicketRuntime_AlreadyConverted", "The reservation has already been converted to a ticket.");
    public static string ReasonRequired => Get("TicketRuntime_ReasonRequired", "A reason is required.");
    public static string CustomInputsInvalid => Get("TicketRuntime_CustomInputsInvalid", "One or more service custom input values are invalid.");
    public static string ReservationTerminal => Get("TicketRuntime_ReservationTerminal", "The reservation is in a terminal state.");
    public static string DifferentReservationDay => Get("TicketRuntime_DifferentReservationDay", "A NoShow reservation can only be converted on its reservation day.");
    public static string ReservationTimeInvalid => Get("TicketRuntime_ReservationTimeInvalid", "The reservation time must be in the future and within the service schedule.");
    public static string AutomaticNoShowCancellation => Get("TicketRuntime_AutomaticNoShowCancellation", "Cancelled automatically after the branch NoShow timeout.");
}
