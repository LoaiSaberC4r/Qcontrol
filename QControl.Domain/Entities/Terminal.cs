using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Terminal : AggregateRoot<int>
{
    public string SerialNo { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    public string IPAddress { get; private set; } = string.Empty;

    public int BranchId { get; private set; }

    public int WindowId { get; private set; }

    public Window Window { get; private set; } = null!;

    public string Type { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public DateTime? DeactivatedOnUtc { get; private set; }

    public Guid? DeactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? DeactivatedByApplicationUser { get; private set; }

    public DateTime? ReactivatedOnUtc { get; private set; }

    public Guid? ReactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? ReactivatedByApplicationUser { get; private set; }

    private Terminal()
    {
    }

    public static Terminal Create(
        int branchId,
        int windowId,
        string number,
        string ipAddress,
        string serialNo,
        string type,
        Guid createdByApplicationUserId)
    {
        return new Terminal
        {
            BranchId = branchId,
            WindowId = windowId,
            Number = number.Trim(),
            IPAddress = ipAddress.Trim(),
            SerialNo = serialNo.Trim(),
            Type = type.Trim(),
            IsActive = true,
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

    public void Deactivate(
        DateTime deactivatedOnUtc,
        Guid deactivatedByApplicationUserId)
    {
        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = deactivatedByApplicationUserId;
        LastModifiedByApplicationUserId =
            deactivatedByApplicationUserId;
    }

    public void Reactivate(
        DateTime reactivatedOnUtc,
        Guid reactivatedByApplicationUserId)
    {
        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = reactivatedByApplicationUserId;
        LastModifiedByApplicationUserId =
            reactivatedByApplicationUserId;
    }
}
