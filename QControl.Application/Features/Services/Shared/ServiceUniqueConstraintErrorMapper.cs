using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceUniqueConstraintErrorMapper
{
    public static bool TryMapCreate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "Services.Create.DuplicateArabicName",
            "Services.Create.DuplicateEnglishName",
            "Services.Create.ServiceCodeAlreadyExists",
            out error);

    public static bool TryMapUpdate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "Services.Update.DuplicateArabicName",
            "Services.Update.DuplicateEnglishName",
            "Services.Update.ServiceCodeAlreadyExists",
            out error);

    public static bool TryMapBranchServiceTreeCreate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "BranchServiceTrees.Create.DuplicateArabicName",
            "BranchServiceTrees.Create.DuplicateEnglishName",
            "BranchServiceTrees.Create.ServiceCodeAlreadyExists",
            out error);

    private static bool TryMap(
        DbUpdateException exception,
        string arabicNameCode,
        string englishNameCode,
        string serviceCodeCode,
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
                arabicNameCode,
                ServiceFeatureMessages.DuplicateArabicName,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                englishNameCode,
                ServiceFeatureMessages.DuplicateEnglishName,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_Service_ServiceCode",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                serviceCodeCode,
                ServiceFeatureMessages.ServiceCodeAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        error = new Error(
            "Services.UniqueConstraint",
            ServiceFeatureMessages.DuplicateEnglishName,
            ErrorType.Conflict);

        return true;
    }
}
