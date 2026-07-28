namespace Qcontrol.Api.Contracts.Segments;

public sealed class CreateGlobalSegmentRequest
{
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
}

public sealed class CreateBranchSegmentRequest
{
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
}

public sealed class UpdateSegmentRequest
{
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}
