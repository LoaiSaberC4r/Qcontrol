using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchBranding : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string? LogoPath { get; private set; }

    public string? MainColor { get; private set; }

    public string? SecondaryColor { get; private set; }

    public string? BackgroundColor { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private BranchBranding()
    {
    }

    public static BranchBranding Create(
        int branchId,
        Guid createdByApplicationUserId)
    {
        return new BranchBranding
        {
            BranchId = branchId,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void UpdateTheme(
        string mainColor,
        string secondaryColor,
        string backgroundColor,
        Guid lastModifiedByApplicationUserId)
    {
        MainColor = NormalizeColor(mainColor);
        SecondaryColor = NormalizeColor(secondaryColor);
        BackgroundColor = NormalizeColor(backgroundColor);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void ReplaceLogo(
        string logoPath,
        Guid lastModifiedByApplicationUserId)
    {
        LogoPath = NormalizePath(logoPath);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    private static string NormalizeColor(string value) =>
        value.Trim().ToUpperInvariant();

    private static string NormalizePath(string value) =>
        value.Trim().Replace('\\', '/');
}
