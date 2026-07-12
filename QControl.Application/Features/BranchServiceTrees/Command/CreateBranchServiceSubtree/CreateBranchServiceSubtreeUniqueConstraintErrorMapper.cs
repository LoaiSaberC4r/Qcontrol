using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;

internal static class CreateBranchServiceSubtreeUniqueConstraintErrorMapper
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

        if (message.Contains(
                "UX_Service_ArabicName_ParentServiceId_Scope_OwnerBranchId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                "BranchServiceTrees.CreateSubtree.DuplicateArabicName",
                ServiceFeatureMessages
                    .BranchServiceSubtreeDuplicateArabicName,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                "BranchServiceTrees.CreateSubtree.DuplicateEnglishName",
                ServiceFeatureMessages
                    .BranchServiceSubtreeDuplicateEnglishName,
                ErrorType.Conflict);

            return true;
        }

        error = new Error(
            "BranchServiceTrees.CreateSubtree.PersistenceConflict",
            ServiceFeatureMessages.BranchServiceSubtreePersistenceConflict,
            ErrorType.Conflict);

        return true;
    }
}
