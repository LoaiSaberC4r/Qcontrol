using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

internal static class BranchVideoMessages
{
    public static string VideoRequired => Get("BranchVideo_Video_Required");
    public static string InvalidVideoType => Get("BranchVideo_InvalidVideoType");
    public static string InvalidDisplayOrder => Get("BranchVideo_InvalidDisplayOrder");
    public static string DuplicateVideoId => Get("BranchVideo_DuplicateVideoId");
    public static string DuplicateDisplayOrder => Get("BranchVideo_DuplicateDisplayOrder");
    public static string NotFound => Get("BranchVideo_NotFound");
    public static string OwnershipMismatch => Get("BranchVideo_DoesNotBelongToBranch");
    public static string AlreadyActive => Get("BranchVideo_AlreadyActive");
    public static string AlreadyInactive => Get("BranchVideo_AlreadyInactive");
    public static string MustBeInactive => Get("BranchVideo_PermanentDelete_MustBeInactive");
    public static string MediaSaveFailed => Get("BranchVideo_MediaSaveFailed");
    public static string PersistenceFailed => Get("BranchVideo_PersistenceFailed");
    public static string OrderConflict => Get("BranchVideo_OrderConflict");
    public static string Uploaded => Get("BranchVideo_Upload_Success");
    public static string Deactivated => Get("BranchVideo_Deactivate_Success");
    public static string Reactivated => Get("BranchVideo_Reactivate_Success");
    public static string Deleted => Get("BranchVideo_Delete_Success");
    public static string Reordered => Get("BranchVideo_Reorder_Success");

    private static string Get(string name) =>
        ErrorMessage.ResourceManager.GetString(name, ErrorMessage.Culture)
        ?? name;
}
