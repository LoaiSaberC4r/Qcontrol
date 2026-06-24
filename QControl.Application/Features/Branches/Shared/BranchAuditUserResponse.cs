public sealed record BranchAuditUserResponse
{
    public Guid ApplicationUserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}