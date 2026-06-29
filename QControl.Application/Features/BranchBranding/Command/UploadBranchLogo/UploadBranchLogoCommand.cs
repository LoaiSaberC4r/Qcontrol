using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Qcontrol.Application.Features.BranchBranding.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;

public sealed record UploadBranchLogoCommand
    : ICommand<BranchBrandingResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public IFormFile? Logo { get; init; }

    public string? RowVersion { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchBranding,
        OperationalCacheTags.BranchBrandingForBranch(BranchId)
    };
}
