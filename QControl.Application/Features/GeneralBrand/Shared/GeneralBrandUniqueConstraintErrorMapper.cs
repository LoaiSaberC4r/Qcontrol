using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.GeneralBrand.Shared;

internal static class GeneralBrandUniqueConstraintErrorMapper
{
    private const string SingletonIndexName =
        "UX_GeneralBrand_SingletonKey";

    public static bool TryMap(DbUpdateException exception, out Error error)
    {
        error = default!;

        if (exception.InnerException is not SqlException
            {
                Number: 2601 or 2627
            } sqlException ||
            !sqlException.Message.Contains(
                SingletonIndexName,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        error = new Error(
            "GeneralBrand.AlreadyConfigured",
            GeneralBrandFeatureMessages.AlreadyConfigured,
            ErrorType.Conflict);

        return true;
    }
}
