using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Shared;

internal static class DisplayUniqueConstraintErrorMapper
{
    public static bool TryMapCreate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "Displays.Create.NumberAlreadyExistsInBranch",
            "Displays.Create.IPAddressAlreadyExistsInBranch",
            "Displays.Create.SerialNoAlreadyExistsInBranch",
            out error);

    public static bool TryMapUpdate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "Displays.Update.NumberAlreadyExistsInBranch",
            "Displays.Update.IPAddressAlreadyExistsInBranch",
            "Displays.Update.SerialNoAlreadyExistsInBranch",
            out error);

    public static bool TryMapRestore(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "Displays.Restore.NumberConflict",
            "Displays.Restore.IPAddressConflict",
            "Displays.Restore.SerialNoConflict",
            out error);

    private static bool TryMap(
        DbUpdateException exception,
        string numberCode,
        string ipAddressCode,
        string serialNoCode,
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

        if (message.Contains(
                "IX_Display_BranchId_Number",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: numberCode,
                Message: ErrorMessage.Display_Number_AlreadyExistsInBranch,
                Type: ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "IX_Display_BranchId_IPAddress",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: ipAddressCode,
                Message:
                    ErrorMessage.Display_IPAddress_AlreadyExistsInBranch,
                Type: ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "IX_Display_BranchId_SerialNo",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                Code: serialNoCode,
                Message:
                    ErrorMessage.Display_SerialNo_AlreadyExistsInBranch,
                Type: ErrorType.Conflict);

            return true;
        }

        error = new Error(
            Code: "Displays.UniqueConstraint",
            Message: ErrorMessage.Display_UniqueConstraint_Conflict,
            Type: ErrorType.Conflict);

        return true;
    }
}
