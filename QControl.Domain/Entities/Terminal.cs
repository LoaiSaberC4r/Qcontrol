using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Terminal : AggregateRoot<int>
{
    public string? SerialNo { get; private set; }

    public int Number { get; private set; }

    public string IPAddress { get; private set; } = string.Empty;

    public int WindowId { get; private set; }

    public Window Window { get; private set; } = null!;

    public string? Type { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private Terminal()
    {
    }

    public static Terminal Create(
        int windowId,
        int number,
        string ipAddress,
        string? serialNo,
        string? type,
        Guid createdByApplicationUserId)
    {
        return new Terminal
        {
            WindowId = windowId,
            Number = number,
            IPAddress = ipAddress.Trim(),
            SerialNo = NormalizeOptional(serialNo),
            Type = NormalizeOptional(type),
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        int number,
        string ipAddress,
        string? serialNo,
        string? type,
        Guid lastModifiedByApplicationUserId)
    {
        Number = number;
        IPAddress = ipAddress.Trim();
        SerialNo = NormalizeOptional(serialNo);
        Type = NormalizeOptional(type);
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}