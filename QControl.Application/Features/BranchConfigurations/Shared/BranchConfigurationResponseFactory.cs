using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Shared;

internal static class BranchConfigurationResponseFactory
{
    public static BranchConfigurationResponse FromEntity(
        BranchConfiguration configuration,
        string? message = null)
    {
        return new BranchConfigurationResponse
        {
            BranchId = configuration.BranchId,
            AllowedTime = configuration.AllowedTime,
            MaximumTicketCallAttempts = configuration.MaximumTicketCallAttempts,
            TicketNoShowAutoCancellationMinutes = configuration.TicketNoShowAutoCancellationMinutes,
            TicketArchiveRetentionDays = configuration.TicketArchiveRetentionDays,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(
                configuration.RowVersion),
            Message = message
        };
    }

    public static BranchConfigurationResponse NotConfigured(int branchId)
    {
        return new BranchConfigurationResponse
        {
            BranchId = branchId,
            AllowedTime = TimeSpan.Zero,
            MaximumTicketCallAttempts = 3,
            TicketNoShowAutoCancellationMinutes = 30,
            TicketArchiveRetentionDays = 30,
            IsConfigured = false,
            RowVersion = null,
            Message = null
        };
    }
}
