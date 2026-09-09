using System.Globalization;
using Qcontrol.Domain.Resources;
using QControl.Domain.Enums;

namespace QControl.Application.Features.TicketRuntime.Shared;

public static class TicketPrintContentFormatter
{
    private static readonly CultureInfo ArabicCulture = CultureInfo.GetCultureInfo("ar");
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en");

    public static string SelectLanguage(
        string arabicValue,
        string englishValue,
        TicketPrintLanguage language) =>
        language == TicketPrintLanguage.Ar ? arabicValue : englishValue;

    public static string FormatServiceFullTree(
        IEnumerable<TicketPrintLocalizedValue> hierarchy,
        TicketPrintLanguage language) =>
        string.Join(" > ", hierarchy.Select(x => SelectLanguage(
            x.ArabicValue, x.EnglishValue, language)));

    public static string FormatTicketNumber(
        string ticketNumber,
        TicketPrintLanguage language)
    {
        var culture = Culture(language);
        var fallback = language == TicketPrintLanguage.Ar
            ? "رقم التذكرة: {0}"
            : "Ticket Number: {0}";
        return string.Format(culture,
            ErrorMessage.ResourceManager.GetString("TicketPrint_TicketNumber_Format", culture)
            ?? fallback, ticketNumber);
    }

    public static string FormatEstimatedWaitingDuration(
        int ticketsAhead,
        int waitingDurationMinutes,
        TicketPrintLanguage language)
    {
        var minutes = checked(ticketsAhead * waitingDurationMinutes);
        var culture = Culture(language);
        var fallback = language == TicketPrintLanguage.Ar
            ? "مدة الانتظار المتوقعة: {0} دقيقة"
            : "Estimated waiting time: {0} minutes";
        return string.Format(culture,
            ErrorMessage.ResourceManager.GetString(
                "TicketPrint_EstimatedWaitingTime_Format", culture) ?? fallback, minutes);
    }

    public static string FormatCustomInputsOrField(
        IEnumerable<TicketPrintCustomInputSnapshot> customInputs,
        string? field,
        TicketPrintLanguage language)
    {
        var ordered = customInputs
            .OrderBy(x => x.OrderSnapshot is null)
            .ThenBy(x => x.OrderSnapshot)
            .ThenBy(x => x.StableId)
            .ToArray();
        if (ordered.Length == 0)
        {
            return field ?? string.Empty;
        }

        return string.Join(Environment.NewLine, ordered.Select(input =>
        {
            var localized = language == TicketPrintLanguage.Ar
                ? input.LabelArSnapshot
                : input.LabelEnSnapshot;
            var label = string.IsNullOrWhiteSpace(localized)
                ? input.NameSnapshot
                : localized.Trim();
            return $"{label}: {input.Value}";
        }));
    }

    private static CultureInfo Culture(TicketPrintLanguage language) =>
        language == TicketPrintLanguage.Ar ? ArabicCulture : EnglishCulture;
}

public sealed record TicketPrintCustomInputSnapshot(
    int StableId,
    string NameSnapshot,
    string? LabelEnSnapshot,
    string? LabelArSnapshot,
    string Value,
    int? OrderSnapshot);

public sealed record TicketPrintLocalizedValue(
    string ArabicValue,
    string EnglishValue);
