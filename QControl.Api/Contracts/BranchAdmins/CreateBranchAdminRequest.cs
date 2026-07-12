namespace Qcontrol.Api.Contracts.BranchAdmins;

public sealed class CreateBranchAdminRequest
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string? PhoneNumber { get; init; }

    public string TemporaryPassword { get; init; } = string.Empty;
}
