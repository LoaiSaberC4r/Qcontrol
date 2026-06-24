using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Location : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string? Governorate { get; private set; }

    public string? City { get; private set; }

    public string? Area { get; private set; }

    public string? Address { get; private set; }

    public string? Longitude { get; private set; }

    public string? Latitude { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private Location()
    {
    }

    internal static Location Create(
        Branch branch,
        string? governorate,
        string? city,
        string? area,
        string? address,
        string? longitude,
        string? latitude,
        Guid createdByApplicationUserId)
    {
        ArgumentNullException.ThrowIfNull(branch);

        return new Location
        {
            Branch = branch,
            Governorate = NormalizeOptional(governorate),
            City = NormalizeOptional(city),
            Area = NormalizeOptional(area),
            Address = NormalizeOptional(address),
            Longitude = NormalizeOptional(longitude),
            Latitude = NormalizeOptional(latitude),
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    internal void Update(
        string? governorate,
        string? city,
        string? area,
        string? address,
        string? longitude,
        string? latitude,
        Guid lastModifiedByApplicationUserId)
    {
        Governorate = NormalizeOptional(governorate);
        City = NormalizeOptional(city);
        Area = NormalizeOptional(area);
        Address = NormalizeOptional(address);
        Longitude = NormalizeOptional(longitude);
        Latitude = NormalizeOptional(latitude);
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