using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.BranchConfigurations.Shared;

internal static class BranchConfigurationUniqueConstraintErrorMapper
{
    private const string BranchIdIndexName =
        "UX_BranchConfiguration_BranchId";

    public static bool TryMap(DbUpdateException exception, out Error error)
    {
        error = default!;

        if (exception.InnerException is not SqlException
            {
                Number: 2601 or 2627
            } sqlException ||
            !sqlException.Message.Contains(
                BranchIdIndexName,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        error = new Error(
            "BranchConfigurations.Create.AlreadyExists",
            BranchConfigurationFeatureMessages.AlreadyExists,
            ErrorType.Conflict);

        return true;
    }
}
