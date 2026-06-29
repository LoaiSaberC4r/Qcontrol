using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchBranding.Shared;

internal static class BranchBrandingUniqueConstraintErrorMapper
{
    public static bool TryMap(DbUpdateException exception, out Error error)
    {
        error = default!;

        if (exception.InnerException is not SqlException
            {
                Number: 2601 or 2627
            })
        {
            return false;
        }

        error = new Error(
            "BranchBranding.ConcurrencyConflict",
            ErrorMessage.Concurrency_Conflict,
            ErrorType.Conflict);

        return true;
    }
}
