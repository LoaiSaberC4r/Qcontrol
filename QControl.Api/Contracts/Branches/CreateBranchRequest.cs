namespace Qcontrol.Api.Contracts.Branches;

public sealed class CreateBranchRequest
{
    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string? License { get; init; }

    public BranchLocationRequest Location { get; init; } = new();
}
