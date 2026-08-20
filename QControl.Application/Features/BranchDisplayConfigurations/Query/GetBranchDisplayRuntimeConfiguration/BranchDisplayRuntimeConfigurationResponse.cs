namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

public sealed class BranchDisplayRuntimeConfigurationResponse
{
    public int BranchId { get; init; }
    public BranchDisplayRuntimeBrandingResponse Branding { get; init; } = new();
    public BranchDisplayRuntimeVisualConfigurationResponse? DisplayConfiguration { get; init; }
    public IReadOnlyList<BranchDisplayRuntimeMessageResponse> Messages { get; init; } =
        Array.Empty<BranchDisplayRuntimeMessageResponse>();
}

public sealed class BranchDisplayRuntimeBrandingResponse
{
    public string? LogoUrl { get; init; }
}

public sealed class BranchDisplayRuntimeVisualConfigurationResponse
{
    public string DisplayBackgroundColor { get; init; } = string.Empty;
    public string MainTitleAr { get; init; } = string.Empty;
    public string MainTitleEn { get; init; } = string.Empty;
    public string HeaderBackgroundColor { get; init; } = string.Empty;
    public string MainTitleTextColor { get; init; } = string.Empty;
    public int MainTitleFontSize { get; init; }
    public string TableHeaderBackgroundColor { get; init; } = string.Empty;
    public string TableHeaderTextColor { get; init; } = string.Empty;
    public string TableRowBackgroundColor { get; init; } = string.Empty;
    public string TableRowTextColor { get; init; } = string.Empty;
    public string TicketNumberBackgroundColor { get; init; } = string.Empty;
    public string TicketNumberTextColor { get; init; } = string.Empty;
    public string TicketColumnTitleAr { get; init; } = string.Empty;
    public string TicketColumnTitleEn { get; init; } = string.Empty;
    public string ServiceColumnTitleAr { get; init; } = string.Empty;
    public string ServiceColumnTitleEn { get; init; } = string.Empty;
    public string WindowColumnTitleAr { get; init; } = string.Empty;
    public string WindowColumnTitleEn { get; init; } = string.Empty;
    public string TickerBackgroundColor { get; init; } = string.Empty;
    public string TickerTextColor { get; init; } = string.Empty;
    public int TickerFontSize { get; init; }
    public bool ShowClock { get; init; }
    public string ClockBackgroundColor { get; init; } = string.Empty;
    public string ClockTextColor { get; init; } = string.Empty;
}

public sealed class BranchDisplayRuntimeMessageResponse
{
    public int Id { get; init; }
    public string TextAr { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
