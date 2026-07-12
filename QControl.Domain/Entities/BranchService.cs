using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchService : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    private BranchService()
    {
    }

    public static BranchService Create(
        int branchId,
        int serviceId,
        Guid createdByApplicationUserId)
    {
        return new BranchService
        {
            BranchId = branchId,
            ServiceId = serviceId,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public static BranchService Create(
        int branchId,
        Service service,
        Guid createdByApplicationUserId)
    {
        return new BranchService
        {
            BranchId = branchId,
            Service = service,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }
}
