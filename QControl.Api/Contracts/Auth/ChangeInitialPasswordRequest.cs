namespace Qcontrol.Api.Contracts.Auth;

public sealed class ChangeInitialPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
