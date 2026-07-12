using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Shared;

internal static class BranchServiceUniqueConstraintErrorMapper
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

        if (!sqlException.Message.Contains(
                "UX_BranchService_BranchId_ServiceId",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        error = new Error(
            "BranchServices.Assign.DuplicateAssignment",
            ServiceFeatureMessages.BranchServicesAssignSuccess,
            ErrorType.Conflict);

        return true;
    }
}
