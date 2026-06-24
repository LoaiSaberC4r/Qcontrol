using BuildingBlock.Api;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.Auth;
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
}