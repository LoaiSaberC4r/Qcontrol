using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.CreateBranchDisplayConfiguration;

public sealed record CreateBranchDisplayConfigurationCommand
    : BranchDisplayConfigurationCommandBase,
      ICommand<BranchDisplayConfigurationResponse>,
      ICacheInvalidator
{
    public IEnumerable<string> Tags => BranchDisplayCacheTags.ForBranch(BranchId);
}
