using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.MultiTenancy;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestTenantContext : ICurrentTenantContext
{
    public TenantMode Mode { get; init; } = TenantMode.Platform;

    public Guid? AccountId { get; init; }

    public Guid? UserId { get; init; } = EntityTestFactory.CurrentUserId;

    public bool IsAuthenticated { get; init; } = true;

    public string? Role { get; init; }

    public UserType UserType { get; init; } = UserType.PlatformAdmin;

    public bool HasAccount => AccountId.HasValue;

    public bool IsPlatformAdmin => true;
}
