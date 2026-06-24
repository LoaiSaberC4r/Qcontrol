using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class DisplayWindow : Entity<int>
{
    public int DisplayId { get; private set; }

    public Display Display { get; private set; } = null!;

    public int WindowId { get; private set; }

    public Window Window { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private DisplayWindow()
    {
    }

    public static DisplayWindow Create(
        int displayId,
        int windowId,
        Guid createdByApplicationUserId)
    {
        return new DisplayWindow
        {
            DisplayId = displayId,
            WindowId = windowId,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void MarkModified(
        Guid lastModifiedByApplicationUserId)
    {
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }
}