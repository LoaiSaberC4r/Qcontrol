using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchBranding.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

public sealed record UpdateBranchThemeCommand
    : ICommand<BranchBrandingResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public string MainColor { get; init; } = string.Empty;

    public string SecondaryColor { get; init; } = string.Empty;

    public string BackgroundColor { get; init; } = string.Empty;

    public string? RowVersion { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchBranding,
        OperationalCacheTags.BranchBrandingForBranch(BranchId)
    };
}
