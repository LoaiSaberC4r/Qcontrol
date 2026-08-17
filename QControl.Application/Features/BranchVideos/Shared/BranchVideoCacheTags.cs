using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

public static class BranchVideoCacheTags
{
    public static string PlaylistKey(int branchId) =>
        $"branch-videos:playlist:{branchId}";

    public static string[] ForBranch(int branchId) =>
    [
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(branchId),
        OperationalCacheTags.BranchVideos,
        OperationalCacheTags.BranchVideosForBranch(branchId)
    ];
}
