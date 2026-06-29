using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Branch : AggregateRoot<int>
{
    private readonly List<WaitingArea> _waitingAreas = new();
    private readonly List<Display> _displays = new();
    private readonly List<BranchAdvertisement> _advertisements = new();

    public string ArabicName { get; private set; } = string.Empty;

    public string EnglishName { get; private set; } = string.Empty;

    public string IPAddress { get; private set; } = string.Empty;

    public string? License { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Location Location { get; private set; } = null!;

    public BranchBranding? Branding { get; private set; }

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

    public IReadOnlyCollection<WaitingArea> WaitingAreas =>
        _waitingAreas.AsReadOnly();

    public IReadOnlyCollection<Display> Displays =>
        _displays.AsReadOnly();

    public IReadOnlyCollection<BranchAdvertisement> Advertisements =>
        _advertisements.AsReadOnly();

    private Branch()
    {
    }

    public static Branch Create(
        string arabicName,
        string englishName,
        string ipAddress,
        string? license,
        string governorate,
        string city,
        string area,
        string address,
        decimal latitude,
        decimal longitude,
        Guid createdByApplicationUserId)
    {
        var branch = new Branch
        {
            ArabicName = NormalizeRequired(arabicName),
            EnglishName = NormalizeRequired(englishName),
            IPAddress = ipAddress.Trim(),
            License = NormalizeOptional(license),
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        branch.Location = Location.Create(
            branch,
            governorate,
            city,
            area,
            address,
            latitude,
            longitude);

        return branch;
    }

    public void Update(
        string arabicName,
        string englishName,
        string ipAddress,
        string? license,
        Guid lastModifiedByApplicationUserId)
    {
        ArabicName = NormalizeRequired(arabicName);
        EnglishName = NormalizeRequired(englishName);
        IPAddress = ipAddress.Trim();
        License = NormalizeOptional(license);
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    public void UpdateLocation(
        string governorate,
        string city,
        string area,
        string address,
        decimal latitude,
        decimal longitude)
    {
        Location.Update(
            governorate,
            city,
            area,
            address,
            latitude,
            longitude);
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

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
