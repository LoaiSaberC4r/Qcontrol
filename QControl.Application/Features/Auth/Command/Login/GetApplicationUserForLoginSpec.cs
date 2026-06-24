using BuildingBlock.Domain.Specification;
using Qcontrol.Domain.Identity;

namespace Qcontrol.Application.Features.Auth.Command.Login;

internal sealed class GetApplicationUserForLoginSpec
    : Specification<ApplicationUser>
{
    public GetApplicationUserForLoginSpec(
        string userNameOrEmail)
    {
        var value = userNameOrEmail.Trim();

        AddCriteria(x =>
            x.IsActive &&
            (
                x.UserName == value ||
                x.Email == value
            ));
    }
}