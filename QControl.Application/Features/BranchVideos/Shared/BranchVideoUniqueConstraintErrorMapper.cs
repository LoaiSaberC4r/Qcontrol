using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

internal static class BranchVideoUniqueConstraintErrorMapper
{
    public static bool TryMapOrderConflict(
        DbUpdateException exception,
        out Error error)
    {
        error = default!;

        if (exception.InnerException is not SqlException { Number: 2601 or 2627 })
        {
            return false;
        }

        error = new Error(
            "BranchVideos.OrderConflict",
            BranchVideoMessages.OrderConflict,
            ErrorType.Conflict);
        return true;
    }
}
