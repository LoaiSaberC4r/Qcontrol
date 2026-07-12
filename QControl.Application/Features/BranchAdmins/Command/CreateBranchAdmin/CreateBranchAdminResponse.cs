namespace Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;

public sealed record CreateBranchAdminResponse
{
    public Guid ApplicationUserId { get; init; }

    public Guid BranchAdminId { get; init; }

    public int BranchId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string? PhoneNumber { get; init; }

    public string UserType { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsFirstLogin { get; init; }

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
