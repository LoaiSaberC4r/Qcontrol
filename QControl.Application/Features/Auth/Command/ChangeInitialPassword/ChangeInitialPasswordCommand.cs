using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Dto;

namespace Qcontrol.Application.Features.Auth.Command.ChangeInitialPassword;

public sealed record ChangeInitialPasswordCommand
    : ICommand<UserTokenDto>
{
    public string NewPassword { get; init; } = string.Empty;
}
