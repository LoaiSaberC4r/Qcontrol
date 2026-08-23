using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal sealed record BranchTicketIssuanceState
    : ITicketIssuanceBrandingLayout
{
    public int BranchId { get; init; }

    public bool IsActive { get; init; }

    public TimeSpan AllowedTime { get; init; }

    public int? BrandingId { get; init; }

    public string? LogoPath { get; init; }

    public string? MainColor { get; init; }

    public string? SecondaryColor { get; init; }

    public string? BackgroundColor { get; init; }

    public string? HeaderColor { get; init; }

    public string? FooterColor { get; init; }

    public string? MainTextColor { get; init; }

    public bool ShowLanguagePage { get; init; }

    public bool DefaultLanguageIsArabic { get; init; }

    public bool AlwaysRequireUserInput { get; init; }

    public bool ShowServiceNavigationPath { get; init; }

    public bool AllowOperatorSelection { get; init; }

    public bool AllowRequestMoreServices { get; init; }

    public string? LanguageButtonBackgroundColor { get; init; }

    public string? LanguageButtonTextColor { get; init; }

    public decimal? LanguageButtonWidth { get; init; }

    public decimal? LanguageButtonHeight { get; init; }

    public string? LanguageButtonText { get; init; }

    public string? ServiceButtonBackgroundColor { get; init; }

    public string? ServiceButtonTextColor { get; init; }

    public decimal? ServiceButtonWidth { get; init; }

    public decimal? ServiceButtonHeight { get; init; }

    public decimal? ServiceButtonSpace { get; init; }

    public decimal? ServiceButtonFontSize { get; init; }

    public string? ServiceButtonText { get; init; }

    public string? KeypadButtonBackgroundColor { get; init; }

    public string? KeypadButtonTextColor { get; init; }

    public decimal? KeypadButtonWidth { get; init; }

    public decimal? KeypadButtonHeight { get; init; }

    public string? KeypadButtonText { get; init; }

    public string? FooterButtonBackgroundColor { get; init; }

    public string? FooterButtonTextColor { get; init; }

    public decimal? FooterButtonWidth { get; init; }

    public decimal? FooterButtonHeight { get; init; }

    public string? FooterButtonText { get; init; }
}

internal sealed record GeneralBrandTicketIssuanceState
    : ITicketIssuanceBrandingLayout
{
    public string? MainColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? BackgroundColor { get; init; }
    public string? HeaderColor { get; init; }
    public string? FooterColor { get; init; }
    public string? MainTextColor { get; init; }
    public bool ShowLanguagePage { get; init; }
    public bool DefaultLanguageIsArabic { get; init; }
    public bool AlwaysRequireUserInput { get; init; }
    public bool ShowServiceNavigationPath { get; init; }
    public bool AllowOperatorSelection { get; init; }
    public bool AllowRequestMoreServices { get; init; }
    public string? LanguageButtonBackgroundColor { get; init; }
    public string? LanguageButtonTextColor { get; init; }
    public decimal? LanguageButtonWidth { get; init; }
    public decimal? LanguageButtonHeight { get; init; }
    public string? LanguageButtonText { get; init; }
    public string? ServiceButtonBackgroundColor { get; init; }
    public string? ServiceButtonTextColor { get; init; }
    public decimal? ServiceButtonWidth { get; init; }
    public decimal? ServiceButtonHeight { get; init; }
    public decimal? ServiceButtonSpace { get; init; }
    public decimal? ServiceButtonFontSize { get; init; }
    public string? ServiceButtonText { get; init; }
    public string? KeypadButtonBackgroundColor { get; init; }
    public string? KeypadButtonTextColor { get; init; }
    public decimal? KeypadButtonWidth { get; init; }
    public decimal? KeypadButtonHeight { get; init; }
    public string? KeypadButtonText { get; init; }
    public string? FooterButtonBackgroundColor { get; init; }
    public string? FooterButtonTextColor { get; init; }
    public decimal? FooterButtonWidth { get; init; }
    public decimal? FooterButtonHeight { get; init; }
    public string? FooterButtonText { get; init; }
}

internal sealed class GetGeneralBrandTicketIssuanceStateSpec
    : Specification<
        QControl.Domain.Entities.GeneralBrand,
        GeneralBrandTicketIssuanceState>
{
    public GetGeneralBrandTicketIssuanceStateSpec()
    {
        UseNoTracking();
        AddCriteria(x => x.SingletonKey == 1);
        Select(x => new GeneralBrandTicketIssuanceState
        {
            MainColor = x.MainColor,
            SecondaryColor = x.SecondaryColor,
            BackgroundColor = x.BackgroundColor,
            HeaderColor = x.HeaderColor,
            FooterColor = x.FooterColor,
            MainTextColor = x.MainTextColor,
            ShowLanguagePage = x.ShowLanguagePage,
            DefaultLanguageIsArabic = x.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = x.AlwaysRequireUserInput,
            ShowServiceNavigationPath = x.ShowServiceNavigationPath,
            AllowOperatorSelection = x.AllowOperatorSelection,
            AllowRequestMoreServices = x.AllowRequestMoreServices,
            LanguageButtonBackgroundColor =
                x.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = x.LanguageButtonTextColor,
            LanguageButtonWidth = x.LanguageButtonWidth,
            LanguageButtonHeight = x.LanguageButtonHeight,
            LanguageButtonText = x.LanguageButtonText,
            ServiceButtonBackgroundColor =
                x.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = x.ServiceButtonTextColor,
            ServiceButtonWidth = x.ServiceButtonWidth,
            ServiceButtonHeight = x.ServiceButtonHeight,
            ServiceButtonSpace = x.ServiceButtonSpace,
            ServiceButtonFontSize = x.ServiceButtonFontSize,
            ServiceButtonText = x.ServiceButtonText,
            KeypadButtonBackgroundColor =
                x.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = x.KeypadButtonTextColor,
            KeypadButtonWidth = x.KeypadButtonWidth,
            KeypadButtonHeight = x.KeypadButtonHeight,
            KeypadButtonText = x.KeypadButtonText,
            FooterButtonBackgroundColor =
                x.FooterButtonBackgroundColor,
            FooterButtonTextColor = x.FooterButtonTextColor,
            FooterButtonWidth = x.FooterButtonWidth,
            FooterButtonHeight = x.FooterButtonHeight,
            FooterButtonText = x.FooterButtonText
        });
    }
}

internal sealed class GetBranchTicketIssuanceStateSpec
    : Specification<Branch, BranchTicketIssuanceState>
{
    public GetBranchTicketIssuanceStateSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(branch => branch.Id == branchId);
        Select(branch => new BranchTicketIssuanceState
        {
            BranchId = branch.Id,
            IsActive = branch.IsActive,
            AllowedTime = branch.Configuration == null
                ? TimeSpan.Zero
                : branch.Configuration.AllowedTime,
            BrandingId = branch.Branding == null
                ? null
                : branch.Branding.Id,
            LogoPath = branch.Branding == null
                ? null
                : branch.Branding.LogoPath,
            MainColor = branch.Branding == null
                ? null
                : branch.Branding.MainColor,
            SecondaryColor = branch.Branding == null
                ? null
                : branch.Branding.SecondaryColor,
            BackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.BackgroundColor,
            HeaderColor = branch.Branding == null
                ? null
                : branch.Branding.HeaderColor,
            FooterColor = branch.Branding == null
                ? null
                : branch.Branding.FooterColor,
            MainTextColor = branch.Branding == null
                ? null
                : branch.Branding.MainTextColor,
            ShowLanguagePage = branch.Branding != null &&
                branch.Branding.ShowLanguagePage,
            DefaultLanguageIsArabic = branch.Branding != null &&
                branch.Branding.DefaultLanguageIsArabic,
            AlwaysRequireUserInput = branch.Branding != null &&
                branch.Branding.AlwaysRequireUserInput,
            ShowServiceNavigationPath = branch.Branding != null &&
                branch.Branding.ShowServiceNavigationPath,
            AllowOperatorSelection = branch.Branding != null &&
                branch.Branding.AllowOperatorSelection,
            AllowRequestMoreServices = branch.Branding != null &&
                branch.Branding.AllowRequestMoreServices,
            LanguageButtonBackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.LanguageButtonBackgroundColor,
            LanguageButtonTextColor = branch.Branding == null
                ? null
                : branch.Branding.LanguageButtonTextColor,
            LanguageButtonWidth = branch.Branding == null
                ? null
                : branch.Branding.LanguageButtonWidth,
            LanguageButtonHeight = branch.Branding == null
                ? null
                : branch.Branding.LanguageButtonHeight,
            LanguageButtonText = branch.Branding == null
                ? null
                : branch.Branding.LanguageButtonText,
            ServiceButtonBackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonBackgroundColor,
            ServiceButtonTextColor = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonTextColor,
            ServiceButtonWidth = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonWidth,
            ServiceButtonHeight = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonHeight,
            ServiceButtonSpace = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonSpace,
            ServiceButtonFontSize = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonFontSize,
            ServiceButtonText = branch.Branding == null
                ? null
                : branch.Branding.ServiceButtonText,
            KeypadButtonBackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.KeypadButtonBackgroundColor,
            KeypadButtonTextColor = branch.Branding == null
                ? null
                : branch.Branding.KeypadButtonTextColor,
            KeypadButtonWidth = branch.Branding == null
                ? null
                : branch.Branding.KeypadButtonWidth,
            KeypadButtonHeight = branch.Branding == null
                ? null
                : branch.Branding.KeypadButtonHeight,
            KeypadButtonText = branch.Branding == null
                ? null
                : branch.Branding.KeypadButtonText,
            FooterButtonBackgroundColor = branch.Branding == null
                ? null
                : branch.Branding.FooterButtonBackgroundColor,
            FooterButtonTextColor = branch.Branding == null
                ? null
                : branch.Branding.FooterButtonTextColor,
            FooterButtonWidth = branch.Branding == null
                ? null
                : branch.Branding.FooterButtonWidth,
            FooterButtonHeight = branch.Branding == null
                ? null
                : branch.Branding.FooterButtonHeight,
            FooterButtonText = branch.Branding == null
                ? null
                : branch.Branding.FooterButtonText
        });
    }
}

internal sealed class GetAssignedBranchServiceIdsSpec
    : Specification<BranchService, int>
{
    public GetAssignedBranchServiceIdsSpec(int branchId)
    {
        UseNoTracking();
        EnableDistinct();
        AddCriteria(assignment => assignment.BranchId == branchId);
        Select(assignment => assignment.ServiceId);
    }
}

internal sealed class GetAllowedScheduledServiceIdsSpec
    : Specification<ServiceSchedule, int>
{
    public GetAllowedScheduledServiceIdsSpec(
        int branchId,
        DayOfWeek currentDay,
        TimeOnly currentTime,
        TimeOnly allowedUntil)
    {
        UseNoTracking();
        EnableDistinct();
        AddCriteria(schedule => schedule.BranchId == branchId);
        AddCriteria(schedule => schedule.TimeSlots.Any(slot =>
            slot.DayOfWeek == currentDay &&
            slot.StartTime <= allowedUntil &&
            slot.EndTime > currentTime));
        Select(schedule => schedule.ServiceId);
    }
}
