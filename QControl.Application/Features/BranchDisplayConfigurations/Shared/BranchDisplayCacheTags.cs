using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

public static class BranchDisplayCacheTags
{
    public static string RuntimeConfigurationKey(int branchId) =>
        $"branch-display:runtime-configuration:{branchId}";

    public static string[] ForBranch(int branchId) =>
    [
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(branchId),
        OperationalCacheTags.BranchBranding,
        OperationalCacheTags.BranchBrandingForBranch(branchId),
        OperationalCacheTags.BranchDisplayConfigurations,
        OperationalCacheTags.BranchDisplayConfigurationForBranch(branchId),
        OperationalCacheTags.BranchDisplayMessages,
        OperationalCacheTags.BranchDisplayMessagesForBranch(branchId)
    ];
}
