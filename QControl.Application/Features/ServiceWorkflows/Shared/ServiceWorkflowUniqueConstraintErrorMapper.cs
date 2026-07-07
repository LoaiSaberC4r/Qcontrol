using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.ServiceWorkflows.Shared;

internal static class ServiceWorkflowUniqueConstraintErrorMapper
{
    public static bool TryMapCreate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "ServiceWorkflows.Create.DuplicateArabicName",
            "ServiceWorkflows.Create.DuplicateEnglishName",
            "ServiceWorkflows.Create.DuplicateStepOrder",
            out error);

    public static bool TryMapUpdate(
        DbUpdateException exception,
        out Error error)
        => TryMap(
            exception,
            "ServiceWorkflows.Update.DuplicateArabicName",
            "ServiceWorkflows.Update.DuplicateEnglishName",
            "ServiceWorkflows.Update.DuplicateStepOrder",
            out error);

    private static bool TryMap(
        DbUpdateException exception,
        string arabicNameCode,
        string englishNameCode,
        string stepOrderCode,
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
                "UX_ServiceWorkflows_ArabicName",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                arabicNameCode,
                ServiceWorkflowMessages.NameAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_ServiceWorkflows_EnglishName",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                englishNameCode,
                ServiceWorkflowMessages.NameAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_ServiceWorkflowSteps_ServiceWorkflowId_StepOrder",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                stepOrderCode,
                ServiceWorkflowMessages.StepOrderDuplicate,
                ErrorType.Conflict);

            return true;
        }

        error = new Error(
            "ServiceWorkflows.UniqueConstraint",
            ServiceWorkflowMessages.NameAlreadyExists,
            ErrorType.Conflict);

        return true;
    }
}
