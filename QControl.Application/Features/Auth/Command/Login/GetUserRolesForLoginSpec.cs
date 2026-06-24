using BuildingBlock.Domain.Specification;
using Qcontrol.Domain.Identity;

namespace Qcontrol.Application.Features.Auth.Command.Login;

internal sealed record UserRoleForLoginDto
{
    public Guid RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;
}

internal sealed class GetUserRolesForLoginSpec
    : Specification<UserRole, UserRoleForLoginDto>
{
    public GetUserRolesForLoginSpec(
        Guid applicationUserId)
    {
        AddCriteria(x =>
            x.ApplicationUserId == applicationUserId);

        Select(x => new UserRoleForLoginDto
        {
            RoleId = x.RoleId,
            RoleName = x.Role.Name
        });
    }
}