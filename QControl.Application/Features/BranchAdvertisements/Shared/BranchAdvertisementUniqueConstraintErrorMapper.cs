using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchAdvertisements.Shared;

internal static class BranchAdvertisementUniqueConstraintErrorMapper
{
    public static bool TryMapOrderConflict(
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

        error = new Error(
            sqlException.Message.Contains(
                "IX_BranchAdvertisement_BranchId_DisplayOrder",
                StringComparison.OrdinalIgnoreCase)
                ? "BranchAdvertisements.OrderConflict"
                : "BranchAdvertisements.UniqueConstraint",
            BranchFeatureMessages.AdvertisementOrderConflict,
            ErrorType.Conflict);

        return true;
    }
}
