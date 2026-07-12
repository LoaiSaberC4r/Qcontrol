using BuildingBlock.Domain.EntitiesHelper;
using QControl.Domain.Entities;

namespace Qcontrol.Domain.Identity;

public sealed class ApplicationUserBranch : Entity<Guid>
{
    public Guid ApplicationUserId { get; private set; }

    public ApplicationUser ApplicationUser { get; private set; } = null!;

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    private ApplicationUserBranch()
    {
    }

    private ApplicationUserBranch(Guid id)
        : base(id)
    {
    }

    public static ApplicationUserBranch Create(
        Guid applicationUserId,
        int branchId,
        Guid createdByApplicationUserId)
    {
        return new ApplicationUserBranch(Guid.NewGuid())
        {
            ApplicationUserId = applicationUserId,
            BranchId = branchId,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }
}
