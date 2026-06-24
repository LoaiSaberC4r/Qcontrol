using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qcontrol.Application.Features.Auth.Command.Login
{
    public sealed record LoginCommand : ICommand<UserTokenDto>
    {
        public string UserNameOrEmail { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}
