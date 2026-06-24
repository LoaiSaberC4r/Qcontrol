using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Display : AggregateRoot<int>
{
    private readonly List<DisplayWindow> _displayWindows = new();

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string? SerialNo { get; private set; }

    public int Number { get; private set; }

    public string IPAddress { get; private set; } = string.Empty;

    public string? Type { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public IReadOnlyCollection<DisplayWindow> DisplayWindows =>
        _displayWindows.AsReadOnly();

    private Display()
    {
    }

    public static Display Create(
        int branchId,
        int number,
        string ipAddress,
        string? serialNo,
        string? type,
        Guid createdByApplicationUserId)
    {
        return new Display
        {
            BranchId = branchId,
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