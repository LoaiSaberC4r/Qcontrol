using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.TicketConfigurations;

public sealed class TicketPrintContentFormatterTests
{
    [Theory]
    [InlineData(TicketPrintLanguage.Ar, "فرع مدينة نصر")]
    [InlineData(TicketPrintLanguage.En, "Nasr City Branch")]
    public void SelectLanguage_uses_configured_print_language(
        TicketPrintLanguage language, string expected)
    {
        Assert.Equal(expected, TicketPrintContentFormatter.SelectLanguage(
            "فرع مدينة نصر", "Nasr City Branch", language));
    }

    [Fact]
    public void Ticket_number_and_waiting_duration_are_localized_explicitly()
    {
        Assert.Equal("رقم التذكرة: A105",
            TicketPrintContentFormatter.FormatTicketNumber("A105", TicketPrintLanguage.Ar));
        Assert.Equal("Ticket Number: A105",
            TicketPrintContentFormatter.FormatTicketNumber("A105", TicketPrintLanguage.En));
        Assert.Equal("مدة الانتظار المتوقعة: 50 دقيقة",
            TicketPrintContentFormatter.FormatEstimatedWaitingDuration(
                10, 5, TicketPrintLanguage.Ar));
        Assert.Equal("Estimated waiting time: 50 minutes",
            TicketPrintContentFormatter.FormatEstimatedWaitingDuration(
                10, 5, TicketPrintLanguage.En));
        Assert.Contains("0", TicketPrintContentFormatter.FormatEstimatedWaitingDuration(
            0, 5, TicketPrintLanguage.En));
        Assert.Contains("0", TicketPrintContentFormatter.FormatEstimatedWaitingDuration(
            10, 0, TicketPrintLanguage.En));
    }

    [Fact]
    public void Full_service_tree_uses_root_to_leaf_order_and_confirmed_separator()
    {
        var hierarchy = new[]
        {
            new TicketPrintLocalizedValue("الخدمات البنكية", "Banking Services"),
            new TicketPrintLocalizedValue("الحسابات", "Accounts"),
            new TicketPrintLocalizedValue("فتح حساب", "Open Account")
        };

        Assert.Equal("الخدمات البنكية > الحسابات > فتح حساب",
            TicketPrintContentFormatter.FormatServiceFullTree(
                hierarchy, TicketPrintLanguage.Ar));
        Assert.Equal("Banking Services > Accounts > Open Account",
            TicketPrintContentFormatter.FormatServiceFullTree(
                hierarchy, TicketPrintLanguage.En));
    }

    [Fact]
    public void Custom_inputs_use_snapshots_order_and_name_fallback()
    {
        var values = new[]
        {
            new TicketPrintCustomInputSnapshot(20, "phone", "Phone Number", "رقم الهاتف",
                "010", 2),
            new TicketPrintCustomInputSnapshot(10, "national-id", "National ID", "رقم الهوية",
                "298", 1),
            new TicketPrintCustomInputSnapshot(30, "legacy", null, null, "old", null)
        };

        Assert.Equal(string.Join(Environment.NewLine,
                "رقم الهوية: 298", "رقم الهاتف: 010", "legacy: old"),
            TicketPrintContentFormatter.FormatCustomInputsOrField(
                values, "ignored", TicketPrintLanguage.Ar));
        Assert.Equal(string.Join(Environment.NewLine,
                "National ID: 298", "Phone Number: 010", "legacy: old"),
            TicketPrintContentFormatter.FormatCustomInputsOrField(
                values, "ignored", TicketPrintLanguage.En));
    }

    [Fact]
    public void Field_is_printed_without_a_label_when_there_are_no_custom_inputs()
    {
        Assert.Equal("REF-42", TicketPrintContentFormatter.FormatCustomInputsOrField(
            Array.Empty<TicketPrintCustomInputSnapshot>(), "REF-42", TicketPrintLanguage.En));
        Assert.Equal(string.Empty, TicketPrintContentFormatter.FormatCustomInputsOrField(
            Array.Empty<TicketPrintCustomInputSnapshot>(), null, TicketPrintLanguage.Ar));
    }
}
