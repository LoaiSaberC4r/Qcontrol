namespace Qcontrol.Application.Features.BranchBranding.Shared;

public sealed class BranchBrandingResponse
{
    public int BranchId { get; init; }

    public string? LogoUrl { get; init; }

    public string? MainColor { get; init; }

    public string? SecondaryColor { get; init; }

    public string? BackgroundColor { get; init; }

    public bool IsConfigured { get; init; }

    public string? RowVersion { get; init; }

    public string? Message { get; init; }
}
