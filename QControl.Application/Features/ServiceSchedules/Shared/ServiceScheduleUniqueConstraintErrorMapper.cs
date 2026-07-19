using BuildingBlock.Domain.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qcontrol.Application.Features.ServiceSchedules.Shared;

internal static class ServiceScheduleUniqueConstraintErrorMapper
{
    public static bool TryMapCreate(
        DbUpdateException exception,
        out Error error)
        => TryMap(exception, "Create", out error);

    public static bool TryMapUpdate(
        DbUpdateException exception,
        out Error error)
        => TryMap(exception, "Update", out error);

    private static bool TryMap(
        DbUpdateException exception,
        string operation,
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
                "UX_ServiceSchedule_BranchId_ServiceId",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                $"ServiceSchedules.{operation}.ScheduleAlreadyExists",
                ServiceScheduleMessages.ScheduleAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_ServiceSchedule_BranchId_SlotCode",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                $"ServiceSchedules.{operation}.SlotCodeAlreadyExists",
                ServiceScheduleMessages.SlotCodeAlreadyExists,
                ErrorType.Conflict);

            return true;
        }

        if (message.Contains(
                "UX_ServiceScheduleWorkDay_ServiceScheduleId_DayOfWeek",
                StringComparison.OrdinalIgnoreCase))
        {
            error = new Error(
                $"ServiceSchedules.{operation}.DuplicateWorkDay",
                ServiceScheduleMessages.DuplicateWorkDay,
                ErrorType.Conflict);

            return true;
        }

        error = new Error(
            $"ServiceSchedules.{operation}.PersistenceConflict",
            ServiceScheduleMessages.ScheduleAlreadyExists,
            ErrorType.Conflict);

        return true;
    }
}
