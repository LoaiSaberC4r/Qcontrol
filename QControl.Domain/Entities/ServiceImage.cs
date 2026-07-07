using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class ServiceImage : Entity<int>
{
    private ServiceImage()
    {
    }

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public string ImagePath { get; private set; } = string.Empty;

    public ServiceImageType ImageType { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public static ServiceImage Create(
        int serviceId,
        string imagePath,
        ServiceImageType imageType,
        int displayOrder,
        Guid createdByApplicationUserId)
    {
        return new ServiceImage
        {
            ServiceId = serviceId,
            ImagePath = NormalizePath(imagePath),
            ImageType = imageType,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public void UpdateDisplayOrder(
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        DisplayOrder = displayOrder;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void Replace(
        string imagePath,
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        ImagePath = NormalizePath(imagePath);
        DisplayOrder = displayOrder;
        IsActive = true;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void Deactivate(Guid lastModifiedByApplicationUserId)
    {
        IsActive = false;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    private static string NormalizePath(string value)
    {
        return value.Trim().Replace('\\', '/').TrimStart('/');
    }
}
