using Qcontrol.Domain.Resources;

namespace QControl.Application.Features.TicketConfigurations.Shared;

internal static class TicketConfigurationFeatureMessages
{
    public static string NotFound => Get("TicketPrintConfiguration_NotFound", "Ticket print configuration was not found.");
    public static string AlreadyExists => Get("TicketPrintConfiguration_AlreadyExists", "Ticket print configuration has already been initialized.");
    public static string InvalidDimensions => Get("TicketPrintConfiguration_InvalidDimensions", "Ticket dimensions must be positive values with at most two decimal places.");
    public static string MissingElement => Get("TicketPrintConfiguration_MissingElement", "All eight fixed ticket elements are required.");
    public static string DuplicateElement => Get("TicketPrintConfiguration_DuplicateElement", "Each fixed ticket element must occur exactly once.");
    public static string UnknownElement => Get("TicketPrintConfiguration_UnknownElement", "The ticket element type is invalid.");
    public static string InvalidLayout => Get("TicketPrintConfiguration_InvalidLayout", "A visible element requires non-negative coordinates and positive dimensions.");
    public static string OutsideBounds => Get("TicketPrintConfiguration_OutsideBounds", "A visible element must fit within the ticket bounds.");
    public static string Overlap => Get("TicketPrintConfiguration_Overlap", "Visible ticket elements cannot overlap.");
    public static string InvalidTypography => Get("TicketPrintConfiguration_InvalidTypography", "A visible text element requires valid typography.");
    public static string LogoTypography => Get("TicketPrintConfiguration_LogoTypography", "BranchLogo does not support typography or language values.");
    public static string InvalidPrecision => Get("TicketPrintConfiguration_InvalidPrecision", "Layout values must fit the supported precision and scale.");
    public static string Created => Get("TicketPrintConfiguration_Create_Success", "Ticket print configuration was created successfully.");
    public static string Updated => Get("TicketPrintConfiguration_Update_Success", "Ticket print configuration was updated successfully.");
    public static string ConcurrencyConflict => Get("TicketPrintConfiguration_ConcurrencyConflict", "The ticket print configuration was changed by another user.");

    private static string Get(string key, string fallback) =>
        ErrorMessage.ResourceManager.GetString(key, ErrorMessage.Culture) ?? fallback;
}
