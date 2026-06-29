using BuildingBlock.Domain.EntitiesHelper;
namespace QControl.Domain.Entities;

public sealed class Location : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string Governorate { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Area { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    private Location()
    {
    }

    internal static Location Create(
        Branch branch,
        string governorate,
        string city,
        string area,
        string address,
        decimal latitude,
        decimal longitude)
    {
        ArgumentNullException.ThrowIfNull(branch);

        return new Location
        {
            Branch = branch,
            Governorate = NormalizeRequired(governorate),
            City = NormalizeRequired(city),
            Area = NormalizeRequired(area),
            Address = NormalizeRequired(address),
            Latitude = latitude,
            Longitude = longitude
        };
    }

    internal void Update(
        string governorate,
        string city,
        string area,
        string address,
        decimal latitude,
        decimal longitude)
    {
        Governorate = NormalizeRequired(governorate);
        City = NormalizeRequired(city);
        Area = NormalizeRequired(area);
        Address = NormalizeRequired(address);
        Latitude = latitude;
        Longitude = longitude;
    }

    private static string NormalizeRequired(string value) => value.Trim();
}
