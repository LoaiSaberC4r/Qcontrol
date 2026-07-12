using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Shared.Security;

namespace Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;

internal static class BranchAdminUniqueConstraintErrorMapper
{
    public static bool TryMap(
        DbUpdateException exception,
        out Error error)
    {
        error = default!;

        if (exception.InnerException is not SqlException
            {
                Number: 2601 or 2627
            } sqlException)
        {
            return false;
        }

        var message = sqlException.Message;

        if (message.Contains("UX_ApplicationUser_UserName", StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: "BranchAdmins.Create.UserNameAlreadyExists",
                Message: IdentityFeatureMessages.UserNameAlreadyExists,
                Type: ErrorType.Conflict);

            return true;
        }

        if (message.Contains("UX_ApplicationUser_Email", StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: "BranchAdmins.Create.EmailAlreadyExists",
                Message: IdentityFeatureMessages.EmailAlreadyExists,
                Type: ErrorType.Conflict);

            return true;
        }

        if (message.Contains("UX_BranchAdmin_ApplicationUserId", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_ApplicationUserBranch_ApplicationUserId_BranchId", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_UserRole_ApplicationUserId_RoleId", StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: "BranchAdmins.Create.IdentityLinkAlreadyExists",
                Message: IdentityFeatureMessages.UserNameAlreadyExists,
                Type: ErrorType.Conflict);

            return true;
        }

        return false;
    }
}
