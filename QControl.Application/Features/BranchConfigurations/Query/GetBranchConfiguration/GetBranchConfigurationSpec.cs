using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;

internal sealed class GetBranchConfigurationSpec
    : Specification<BranchConfiguration, BranchConfigurationResponse>
{
    public GetBranchConfigurationSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();

        Select(x => new BranchConfigurationResponse
        {
            BranchId = x.BranchId,
            AllowedTime = x.AllowedTime,
            IsConfigured = true,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            Message = null
        });
    }
}
