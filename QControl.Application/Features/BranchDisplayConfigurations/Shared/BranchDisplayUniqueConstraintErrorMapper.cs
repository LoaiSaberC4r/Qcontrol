using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

internal static class BranchDisplayUniqueConstraintErrorMapper
{
    public static bool TryMapConfiguration(DbUpdateException exception, out Error error)
    {
        error = default!;
        if (!IsUniqueViolation(exception))
        {
            return false;
        }

        error = new Error(
            "BranchDisplayConfiguration.AlreadyExists",
            BranchDisplayFeatureMessages.ConfigurationAlreadyExists,
            ErrorType.Conflict);
        return true;
    }

    public static bool TryMapMessageOrder(DbUpdateException exception, out Error error)
    {
        error = default!;
        if (!IsUniqueViolation(exception))
        {
            return false;
        }

        error = new Error(
            "BranchDisplayMessage.OrderConflict",
            BranchDisplayFeatureMessages.OrderConflict,
            ErrorType.Conflict);
        return true;
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
