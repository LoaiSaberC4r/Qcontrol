using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

internal static class ServiceGlobalizationRequestUniqueConstraintErrorMapper
{
    public static bool TryMapCreate(
        DbUpdateException exception,
        out Error error)
    {
        error = default!;

        if (!TryGetSqlUniqueException(exception, out var sqlException))
        {
            return false;
        }

        if (sqlException.Message.Contains(
                "UX_ServiceGlobalizationRequest_RootServiceId_Pending",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                "BranchServiceTrees.Create.PendingRequestAlreadyExists",
                ServiceGlobalizationRequestMessages.PendingRequestAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        return false;
    }

    public static bool TryMapReview(
        DbUpdateException exception,
        out Error error)
    {
        return TryMap(
            exception,
            "ServiceGlobalizationRequests.Approve.PendingRequestAlreadyExists",
            "ServiceGlobalizationRequests.Approve.DuplicateArabicName",
            "ServiceGlobalizationRequests.Approve.DuplicateEnglishName",
            out error);
    }

    private static bool TryMap(
        DbUpdateException exception,
        string pendingCode,
        string duplicateArabicCode,
        string duplicateEnglishCode,
        out Error error)
    {
        error = default!;

        if (!TryGetSqlUniqueException(exception, out var sqlException))
        {
            return false;
        }

        var message = sqlException.Message;

        if (message.Contains(
                "UX_ServiceGlobalizationRequest_RootServiceId_Pending",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                pendingCode,
                ServiceGlobalizationRequestMessages.PendingRequestAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_Service_ArabicName_ParentServiceId_Scope_OwnerBranchId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                duplicateArabicCode,
                ServiceGlobalizationRequestMessages.DuplicateGlobalArabicName,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                duplicateEnglishCode,
                ServiceGlobalizationRequestMessages.DuplicateGlobalEnglishName,
                ErrorType.Conflict);

            return true;
        }

        error = new Error(
            "ServiceGlobalizationRequests.UniqueConstraint",
            ServiceGlobalizationRequestMessages.RequestItemMismatch,
            ErrorType.Conflict);

        return true;
    }

    private static bool TryGetSqlUniqueException(
        DbUpdateException exception,
        out SqlException sqlException)
    {
        if (exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } found)
        {
            sqlException = found;
            return true;
        }

        sqlException = default!;
        return false;
    }
}
