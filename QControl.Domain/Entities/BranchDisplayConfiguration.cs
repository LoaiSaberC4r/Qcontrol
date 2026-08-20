using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchDisplayConfiguration : Entity<int>
{
    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;
    public string DisplayBackgroundColor { get; private set; } = string.Empty;
    public string MainTitleAr { get; private set; } = string.Empty;
    public string MainTitleEn { get; private set; } = string.Empty;
    public string HeaderBackgroundColor { get; private set; } = string.Empty;
    public string MainTitleTextColor { get; private set; } = string.Empty;
    public int MainTitleFontSize { get; private set; }
    public string TableHeaderBackgroundColor { get; private set; } = string.Empty;
    public string TableHeaderTextColor { get; private set; } = string.Empty;
    public string TableRowBackgroundColor { get; private set; } = string.Empty;
    public string TableRowTextColor { get; private set; } = string.Empty;
    public string TicketNumberBackgroundColor { get; private set; } = string.Empty;
    public string TicketNumberTextColor { get; private set; } = string.Empty;
    public string TicketColumnTitleAr { get; private set; } = string.Empty;
    public string TicketColumnTitleEn { get; private set; } = string.Empty;
    public string ServiceColumnTitleAr { get; private set; } = string.Empty;
    public string ServiceColumnTitleEn { get; private set; } = string.Empty;
    public string WindowColumnTitleAr { get; private set; } = string.Empty;
    public string WindowColumnTitleEn { get; private set; } = string.Empty;
    public string TickerBackgroundColor { get; private set; } = string.Empty;
    public string TickerTextColor { get; private set; } = string.Empty;
    public int TickerFontSize { get; private set; }
    public bool ShowClock { get; private set; }
    public string ClockBackgroundColor { get; private set; } = string.Empty;
    public string ClockTextColor { get; private set; } = string.Empty;
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    public Guid CreatedByApplicationUserId { get; private set; }
    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;
    public Guid? LastModifiedByApplicationUserId { get; private set; }
    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    private BranchDisplayConfiguration()
    {
    }

    public static BranchDisplayConfiguration Create(
        int branchId,
        BranchDisplayConfigurationSettings settings,
        Guid createdByApplicationUserId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchId));
        }

        if (createdByApplicationUserId == Guid.Empty)
        {
            throw new ArgumentException("A creator is required.", nameof(createdByApplicationUserId));
        }

        var configuration = new BranchDisplayConfiguration
        {
            BranchId = branchId,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
        configuration.Apply(settings, createdByApplicationUserId, isCreation: true);
        return configuration;
    }

    public void Update(
        BranchDisplayConfigurationSettings settings,
        Guid lastModifiedByApplicationUserId)
    {
        if (lastModifiedByApplicationUserId == Guid.Empty)
        {
            throw new ArgumentException("A modifier is required.", nameof(lastModifiedByApplicationUserId));
        }

        Apply(settings, lastModifiedByApplicationUserId, isCreation: false);
    }

    private void Apply(
        BranchDisplayConfigurationSettings settings,
        Guid applicationUserId,
        bool isCreation)
    {
        ArgumentNullException.ThrowIfNull(settings);

        DisplayBackgroundColor = NormalizeColor(settings.DisplayBackgroundColor);
        MainTitleAr = NormalizeRequired(settings.MainTitleAr);
        MainTitleEn = NormalizeRequired(settings.MainTitleEn);
        HeaderBackgroundColor = NormalizeColor(settings.HeaderBackgroundColor);
        MainTitleTextColor = NormalizeColor(settings.MainTitleTextColor);
        MainTitleFontSize = NormalizeFontSize(settings.MainTitleFontSize);
        TableHeaderBackgroundColor = NormalizeColor(settings.TableHeaderBackgroundColor);
        TableHeaderTextColor = NormalizeColor(settings.TableHeaderTextColor);
        TableRowBackgroundColor = NormalizeColor(settings.TableRowBackgroundColor);
        TableRowTextColor = NormalizeColor(settings.TableRowTextColor);
        TicketNumberBackgroundColor = NormalizeColor(settings.TicketNumberBackgroundColor);
        TicketNumberTextColor = NormalizeColor(settings.TicketNumberTextColor);
        TicketColumnTitleAr = NormalizeRequired(settings.TicketColumnTitleAr);
        TicketColumnTitleEn = NormalizeRequired(settings.TicketColumnTitleEn);
        ServiceColumnTitleAr = NormalizeRequired(settings.ServiceColumnTitleAr);
        ServiceColumnTitleEn = NormalizeRequired(settings.ServiceColumnTitleEn);
        WindowColumnTitleAr = NormalizeRequired(settings.WindowColumnTitleAr);
        WindowColumnTitleEn = NormalizeRequired(settings.WindowColumnTitleEn);
        TickerBackgroundColor = NormalizeColor(settings.TickerBackgroundColor);
        TickerTextColor = NormalizeColor(settings.TickerTextColor);
        TickerFontSize = NormalizeFontSize(settings.TickerFontSize);
        ShowClock = settings.ShowClock;
        ClockBackgroundColor = NormalizeColor(settings.ClockBackgroundColor);
        ClockTextColor = NormalizeColor(settings.ClockTextColor);

        if (!isCreation)
        {
            LastModifiedByApplicationUserId = applicationUserId;
        }
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", nameof(value));
        }

        return value.Trim();
    }

    private static string NormalizeColor(string value)
    {
        var normalized = NormalizeRequired(value).ToUpperInvariant();
        if (normalized.Length != 7 || normalized[0] != '#' ||
            normalized.Skip(1).Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException("A hexadecimal RGB color is required.", nameof(value));
        }

        return normalized;
    }

    private static int NormalizeFontSize(int value)
    {
        if (value is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        return value;
    }
}
