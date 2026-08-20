using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;

public sealed record UpdateBranchDisplayConfigurationCommand
    : BranchDisplayConfigurationCommandBase,
      ICommand<BranchDisplayConfigurationResponse>,
      ICacheInvalidator
{
    public string? RowVersion { get; init; }
    public IEnumerable<string> Tags => BranchDisplayCacheTags.ForBranch(BranchId);
}
