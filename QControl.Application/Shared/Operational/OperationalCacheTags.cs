namespace QControl.Application.Shared.Operational;

internal static class OperationalCacheTags
{
    public const string Branches = "branches";
    public const string WaitingAreas = "waiting-areas";
    public const string Windows = "windows";
    public const string Terminals = "terminals";
    public const string Displays = "displays";
    public const string Services = "services";
    public const string ServiceCentral = "service-central";
    public const string BranchServices = "branch-services";
    public const string ServiceGlobalizationRequests =
        "service-globalization-requests";
    public const string ServiceWorkflows = "service-workflows";
    public const string DisplayWindows = "display-windows";
    public const string BranchBranding = "branch-branding";
    public const string BranchAdvertisements = "branch-advertisements";

    public static string Branch(int branchId) => $"branch:{branchId}";

    public static string BranchBrandingForBranch(int branchId) =>
        $"branch:{branchId}:branding";

    public static string BranchAdvertisementsForBranch(int branchId) =>
        $"branch:{branchId}:advertisements";

    public static string WaitingArea(int waitingAreaId) =>
        $"waiting-area:{waitingAreaId}";

    public static string Window(int windowId) => $"window:{windowId}";

    public static string Terminal(int terminalId) => $"terminal:{terminalId}";

    public static string Display(int displayId) => $"display:{displayId}";

    public static string Service(int serviceId) => $"service:{serviceId}";

    public static string BranchServicesForBranch(int branchId) =>
        $"branch:{branchId}:services";

    public static string ServiceGlobalizationRequest(int requestId) =>
        $"service-globalization-request:{requestId}";

    public static string ServiceGlobalizationRequestsForBranch(int branchId) =>
        $"branch:{branchId}:service-globalization-requests";

    public static string ServiceWorkflow(int serviceWorkflowId) =>
        $"service-workflow:{serviceWorkflowId}";

    public static string BranchServiceWorkflows(
        int branchId,
        int leafServiceId)
        => $"service-workflows:branch:{branchId}:service:{leafServiceId}";

    public static string BranchWorkflowCandidates(int branchId) =>
        $"service-workflow-candidates:branch:{branchId}";

    public static string DisplayLinkedWindows(int displayId) =>
        $"display:{displayId}:windows";

    public static string DisplayAvailableWindows(int displayId) =>
        $"display:{displayId}:available-windows";
}
