using BuildingBlock.Application.Abstraction.Security;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; init; } = true;

    public Guid? UserId { get; init; } = EntityTestFactory.CurrentUserId;

    public Guid? AccountId { get; init; }

    public Guid? ActiveBranchId { get; init; }

    public string? Role { get; init; }

    public UserType UserType { get; init; } = UserType.PlatformAdmin;

    public int? UserTypeValue => (int)UserType;
}
