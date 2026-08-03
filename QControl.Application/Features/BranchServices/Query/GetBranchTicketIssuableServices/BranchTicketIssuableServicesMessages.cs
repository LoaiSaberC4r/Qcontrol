using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal static class BranchTicketIssuableServicesMessages
{
    public static string AuthenticationRequired =>
        Get(
            "BranchTicketIssuableServices_AuthenticationRequired",
            "Authentication is required.");

    public static string BranchIdRequired =>
        Get(
            "BranchTicketIssuableServices_BranchIdRequired",
            "A valid branch id is required.");

    public static string BranchNotFound =>
        Get(
            "BranchTicketIssuableServices_BranchNotFound",
            "The selected branch could not be found.");

    public static string BranchInactive =>
        Get(
            "BranchTicketIssuableServices_BranchInactive",
            "The selected branch is inactive and cannot currently issue tickets.");

    private static string Get(string resourceName, string fallback) =>
        ErrorMessage.ResourceManager.GetString(
            resourceName,
            ErrorMessage.Culture) ?? fallback;
}
