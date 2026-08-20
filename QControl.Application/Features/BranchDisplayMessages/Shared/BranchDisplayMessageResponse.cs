namespace Qcontrol.Application.Features.BranchDisplayMessages.Shared;

public sealed class BranchDisplayMessageResponse
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public string TextAr { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

public sealed class BranchDisplayMessageStateResponse
{
    public int MessageId { get; init; }
    public int BranchId { get; init; }
    public bool IsActive { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
