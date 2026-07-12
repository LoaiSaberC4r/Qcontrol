using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Auth;
using Qcontrol.Application.Features.Auth.Command.ChangeInitialPassword;
using Qcontrol.Application.Features.Auth.Command.Login;

namespace Qcontrol.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender sender;

    public AuthController(ISender sender)
    {
        this.sender = sender
            ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            UserNameOrEmail = request.UserNameOrEmail,
            Password = request.Password
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }

    [HttpPost("change-initial-password")]
    [Authorize]
    public async Task<IActionResult> ChangeInitialPassword(
        [FromBody] ChangeInitialPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeInitialPasswordCommand
        {
            NewPassword = request.NewPassword
        };

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.ToIActionResult();
    }
}
