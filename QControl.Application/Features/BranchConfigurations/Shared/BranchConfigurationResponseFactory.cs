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
            IsConfigured = false,
            RowVersion = null,
            Message = null
        };
    }
}
