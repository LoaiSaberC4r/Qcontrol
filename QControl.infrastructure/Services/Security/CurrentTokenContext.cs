using BuildingBlock.Application.Abstraction.Security;
using Microsoft.AspNetCore.Http;
using QControl.Application.Abstraction.Security;

namespace QControl.infrastructure.Services.Security;

internal sealed class CurrentTokenContext : ICurrentTokenContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTokenContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public int? ActiveBranchId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(JwtClaimTypesCustom.ActiveBranchId)?
                .Value;

            return int.TryParse(value, out var branchId) && branchId > 0
                ? branchId
                : null;
        }
    }

    public bool PasswordChangeRequired
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(JwtClaimTypesCustom.PasswordChangeRequired)?
                .Value;

            return bool.TryParse(value, out var required) && required;
        }
    }
}
