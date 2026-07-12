using BuildingBlock.Application.Abstraction.Security;
using Microsoft.AspNetCore.Authorization;

namespace QControl.infrastructure.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var passwordChangeRequired =
                bool.TryParse(
                    context.User.FindFirst(JwtClaimTypesCustom.PasswordChangeRequired)?.Value,
                    out var parsedPasswordChangeRequired)
                && parsedPasswordChangeRequired;

            if (passwordChangeRequired)
            {
                return Task.CompletedTask;
            }

            var userPermissions = context.User
                .FindAll(JwtClaimTypesCustom.Permission)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool hasAnyPermission = requirement.Permissions.Any(p => userPermissions.Contains(p));

            if (hasAnyPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public List<string> Permissions { get; }

        public PermissionRequirement(IEnumerable<string> permissions)
        {
            Permissions = permissions.Select(p => p.Trim()).ToList();
        }
    }
}
