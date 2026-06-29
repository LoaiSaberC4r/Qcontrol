using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class DisplayWindow : Entity<int>
{
    public int DisplayId { get; private set; }

    public Display Display { get; private set; } = null!;

    public int WindowId { get; private set; }

    public Window Window { get; private set; } = null!;

    public int BranchId { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    private DisplayWindow()
    {
    }

    public static DisplayWindow Create(
        int branchId,
        int displayId,
        int windowId,
        Guid createdByApplicationUserId)
    {
        return new DisplayWindow
        {
            BranchId = branchId,
            DisplayId = displayId,
            WindowId = windowId,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }
}
