using BuildingBlock.Domain.EntitiesHelper;
using BuildingBlock.Domain.Primitive;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Terminal : AggregateRoot<int>,
    ISoftDeleteEntity
{
    public string SerialNo { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    public string IPAddress { get; private set; } = string.Empty;

    public int WindowId { get; private set; }

    public Window Window { get; private set; } = null!;

    public string Type { get; private set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOnUtc { get; set; }

    public DateTime? RestoredOnUtc { get; set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private Terminal()
    {
    }

    public static Terminal Create(
        int windowId,
        string number,
        string ipAddress,
        string serialNo,
        string type,
        Guid createdByApplicationUserId)
    {
        return new Terminal
        {
            WindowId = windowId,
            Number = number.Trim(),
            IPAddress = ipAddress.Trim(),
            SerialNo = serialNo.Trim(),
            Type = type.Trim(),
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        string number,
        string ipAddress,
        string serialNo,
        string type,
        Guid lastModifiedByApplicationUserId)
    {
        Number = number.Trim();
        IPAddress = ipAddress.Trim();
        SerialNo = serialNo.Trim();
        Type = type.Trim();
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    public void Restore()
        => IsDeleted = false;
}
