using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchConfigurations.Shared;

internal static class BranchConfigurationFeatureMessages
{
    public static string Created =>
        Get(nameof(Created), "BranchConfiguration_Create_Success");

    public static string Updated =>
        Get(nameof(Updated), "BranchConfiguration_Update_Success");

    public static string AlreadyExists =>
        Get(nameof(AlreadyExists), "BranchConfiguration_AlreadyExists");

    public static string NotFound =>
        Get(nameof(NotFound), "BranchConfiguration_NotFound");

    public static string AllowedTimeRequired =>
        Get(nameof(AllowedTimeRequired), "BranchConfiguration_AllowedTime_Required");

    public static string AllowedTimeOutOfRange =>
        Get(nameof(AllowedTimeOutOfRange), "BranchConfiguration_AllowedTime_OutOfRange");

    public static string BranchNotFound =>
        Get(nameof(BranchNotFound), "BranchConfiguration_Branch_NotFound");

    public static string RowVersionRequired =>
        Get(nameof(RowVersionRequired), "BranchConfiguration_RowVersion_Required");

    public static string InvalidRowVersion =>
        Get(nameof(InvalidRowVersion), "BranchConfiguration_RowVersion_Invalid");

    public static string ConcurrencyConflict =>
        Get(nameof(ConcurrencyConflict), "BranchConfiguration_ConcurrencyConflict");

    private static string Get(string fallback, string resourceName) =>
        ErrorMessage.ResourceManager.GetString(
            resourceName,
            ErrorMessage.Culture) ?? fallback;
}
