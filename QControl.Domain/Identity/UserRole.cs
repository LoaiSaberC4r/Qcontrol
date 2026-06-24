using BuildingBlock.Domain.EntitiesHelper;

namespace Qcontrol.Domain.Identity;

public sealed class UserRole : Entity<Guid>
{
    public Guid ApplicationUserId { get; private set; }

    public ApplicationUser ApplicationUser { get; private set; } = null!;

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    private UserRole()
    {
    }

    private UserRole(Guid id)
        : base(id)
    {
    }

    public static UserRole Create(
        Guid applicationUserId,
        Guid roleId,
        Guid createdByApplicationUserId)
    {
        return new UserRole(Guid.NewGuid())
        {
            ApplicationUserId = applicationUserId,
            RoleId = roleId,
            CreatedByApplicationUserId =
                createdByApplicationUserId
        };
    }
}