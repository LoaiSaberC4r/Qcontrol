namespace Qcontrol.Api.Contracts.Branches;

public sealed class UpdateBranchThemeRequest
{
    public string MainColor { get; init; } = string.Empty;

    public string SecondaryColor { get; init; } = string.Empty;

    public string BackgroundColor { get; init; } = string.Empty;

    public string? RowVersion { get; init; }
}
