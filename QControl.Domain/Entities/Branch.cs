using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Branch : AggregateRoot<int>
{
    private readonly List<WaitingArea> _waitingAreas = new();
    private readonly List<Display> _displays = new();

    public string? ArabicName { get; private set; }

    public string? EnglishName { get; private set; }

    public string IPAddress { get; private set; } = string.Empty;

    public bool IsUpdatesAvailable { get; private set; }

    public DateTime LastUpdated { get; private set; }

    public string? License { get; private set; }

    public Location Location { get; private set; } = null!;

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public IReadOnlyCollection<WaitingArea> WaitingAreas =>
        _waitingAreas.AsReadOnly();

    public IReadOnlyCollection<Display> Displays =>
        _displays.AsReadOnly();

    private Branch()
    {
    }

    public static Branch Create(
        string? arabicName,
        string? englishName,
        string ipAddress,
        string? license,
        string? governorate,
        string? city,
        string? area,
        string? address,
        string? longitude,
        string? latitude,
        Guid createdByApplicationUserId)
    {
        var branch = new Branch
        {
            ArabicName = NormalizeOptional(arabicName),
            EnglishName = NormalizeOptional(englishName),
            IPAddress = ipAddress.Trim(),
            IsUpdatesAvailable = true,
            LastUpdated = DateTime.UtcNow,
            License = NormalizeOptional(license),
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        branch.Location = Location.Create(
            branch,
            governorate,
            city,
            area,
            address,
            longitude,
            latitude,
            createdByApplicationUserId);

        return branch;
    }

    public void Update(
        string? arabicName,
        string? englishName,
        string ipAddress,
        string? license,
        Guid lastModifiedByApplicationUserId)
    {
        ArabicName = NormalizeOptional(arabicName);
        EnglishName = NormalizeOptional(englishName);
        IPAddress = ipAddress.Trim();
        License = NormalizeOptional(license);
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    public void UpdateLocation(
        string? governorate,
        string? city,
        string? area,
        string? address,
        string? longitude,
        string? latitude,
        Guid lastModifiedByApplicationUserId)
    {
        Location.Update(
            governorate,
            city,
            area,
            address,
            longitude,
            latitude,
            lastModifiedByApplicationUserId);
    }

    public void SetUpdateState(
        bool isUpdatesAvailable,
        DateTime lastUpdated,
        Guid lastModifiedByApplicationUserId)
    {
        IsUpdatesAvailable = isUpdatesAvailable;
        LastUpdated = lastUpdated;
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