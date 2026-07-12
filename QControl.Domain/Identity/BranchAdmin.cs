using BuildingBlock.Domain.EntitiesHelper;

namespace Qcontrol.Domain.Identity;

public sealed class BranchAdmin : AggregateRoot<Guid>
{
    public Guid ApplicationUserId { get; private set; }

    public ApplicationUser ApplicationUser { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    private BranchAdmin()
    {
    }

    private BranchAdmin(Guid id)
        : base(id)
    {
    }

    public static BranchAdmin Create(
        Guid applicationUserId,
        Guid createdByApplicationUserId)
    {
        return new BranchAdmin(Guid.NewGuid())
        {
            ApplicationUserId = applicationUserId,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }
}
