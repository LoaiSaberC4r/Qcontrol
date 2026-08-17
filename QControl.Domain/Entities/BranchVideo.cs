using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class BranchVideo : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string OriginalFileName { get; private set; } = string.Empty;

    public string OriginalPath { get; private set; } = string.Empty;

    public string? HlsManifestPath { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    public BranchVideoProcessingStatus ProcessingStatus { get; private set; }

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

    private BranchVideo()
    {
    }

    public static BranchVideo Create(
        int branchId,
        string originalFileName,
        string originalPath,
        int displayOrder,
        Guid createdByApplicationUserId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchId));
        }

        if (createdByApplicationUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A creator is required.",
                nameof(createdByApplicationUserId));
        }

        return new BranchVideo
        {
            BranchId = branchId,
            OriginalFileName = NormalizeRequired(originalFileName),
            OriginalPath = NormalizePath(originalPath),
            HlsManifestPath = null,
            DisplayOrder = NormalizeDisplayOrder(displayOrder),
            IsActive = true,
            ProcessingStatus = BranchVideoProcessingStatus.Processing,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public void ChangeDisplayOrder(
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        DisplayOrder = NormalizeDisplayOrder(displayOrder);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public bool Deactivate(
        DateTime deactivatedOnUtc,
        Guid deactivatedByApplicationUserId)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = deactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = deactivatedByApplicationUserId;
        return true;
    }

    public bool Reactivate(
        DateTime reactivatedOnUtc,
        Guid reactivatedByApplicationUserId)
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = reactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = reactivatedByApplicationUserId;
        return true;
    }

    public void MarkProcessing()
    {
        ProcessingStatus = BranchVideoProcessingStatus.Processing;
        HlsManifestPath = null;
    }

    public void MarkReady(string hlsManifestPath)
    {
        HlsManifestPath = NormalizePath(hlsManifestPath);
        ProcessingStatus = BranchVideoProcessingStatus.Ready;
    }

    public void MarkFailed()
    {
        ProcessingStatus = BranchVideoProcessingStatus.Failed;
        HlsManifestPath = null;
    }

    private static int NormalizeDisplayOrder(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Display order must be greater than zero.");
        }

        return value;
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", nameof(value));
        }

        return value.Trim();
    }

    private static string NormalizePath(string value) =>
        NormalizeRequired(value).Replace('\\', '/').TrimStart('/');
}
