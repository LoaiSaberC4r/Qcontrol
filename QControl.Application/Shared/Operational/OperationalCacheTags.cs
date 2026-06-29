namespace QControl.Application.Shared.Operational;

internal static class OperationalCacheTags
{
    public const string Branches = "branches";
    public const string WaitingAreas = "waiting-areas";
    public const string Windows = "windows";
    public const string Terminals = "terminals";
    public const string Displays = "displays";
    public const string DisplayWindows = "display-windows";

    public static string Branch(int branchId) => $"branch:{branchId}";

    public static string WaitingArea(int waitingAreaId) =>
        $"waiting-area:{waitingAreaId}";

    public static string Window(int windowId) => $"window:{windowId}";

    public static string Terminal(int terminalId) => $"terminal:{terminalId}";

    public static string Display(int displayId) => $"display:{displayId}";

    public static string DisplayLinkedWindows(int displayId) =>
        $"display:{displayId}:windows";

    public static string DisplayAvailableWindows(int displayId) =>
        $"display:{displayId}:available-windows";
}
