using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

internal static class BranchDisplayFeatureMessages
{
    public static string ConfigurationNotFound => Get("BranchDisplayConfiguration_NotFound");
    public static string ConfigurationAlreadyExists => Get("BranchDisplayConfiguration_AlreadyExists");
    public static string ConfigurationCreated => Get("BranchDisplayConfiguration_Created");
    public static string ConfigurationUpdated => Get("BranchDisplayConfiguration_Updated");
    public static string ColorRequired => Get("BranchDisplayConfiguration_ColorRequired");
    public static string InvalidColor => Get("BranchDisplayConfiguration_InvalidColor");
    public static string TitleRequired => Get("BranchDisplayConfiguration_TitleRequired");
    public static string TitleMaxLength => Get("BranchDisplayConfiguration_TitleMaxLength");
    public static string InvalidFontSize => Get("BranchDisplayConfiguration_InvalidFontSize");
    public static string MessageNotFound => Get("BranchDisplayMessage_NotFound");
    public static string MessageOwnershipMismatch => Get("BranchDisplayMessage_DoesNotBelongToBranch");
    public static string MessageCreated => Get("BranchDisplayMessage_Created");
    public static string MessageUpdated => Get("BranchDisplayMessage_Updated");
    public static string MessageDeactivated => Get("BranchDisplayMessage_Deactivated");
    public static string MessageReactivated => Get("BranchDisplayMessage_Reactivated");
    public static string MessageAlreadyActive => Get("BranchDisplayMessage_AlreadyActive");
    public static string MessageAlreadyInactive => Get("BranchDisplayMessage_AlreadyInactive");
    public static string MessageTextRequired => Get("BranchDisplayMessage_TextRequired");
    public static string MessageTextMaxLength => Get("BranchDisplayMessage_TextMaxLength");
    public static string InvalidDisplayOrder => Get("BranchDisplayMessage_InvalidDisplayOrder");
    public static string DuplicateMessageId => Get("BranchDisplayMessage_DuplicateMessageId");
    public static string DuplicateDisplayOrder => Get("BranchDisplayMessage_DuplicateDisplayOrder");
    public static string OrderConflict => Get("BranchDisplayMessage_OrderConflict");

    private static string Get(string name) =>
        ErrorMessage.ResourceManager.GetString(name, ErrorMessage.Culture) ?? name;
}
