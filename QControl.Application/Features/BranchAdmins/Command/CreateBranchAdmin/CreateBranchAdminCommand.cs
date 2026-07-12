using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.BranchAdmins.Command.CreateBranchAdmin;

public sealed record CreateBranchAdminCommand
    : ICommand<CreateBranchAdminResponse>
{
    public int BranchId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string? PhoneNumber { get; init; }

    public string TemporaryPassword { get; init; } = string.Empty;
}
