namespace Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;

public sealed class ServiceSelectionOptionResponse
{
    public int ServiceId { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool HasChildren { get; init; }

    public bool CanIssueTicket { get; init; }

    public bool HasReservation { get; init; }

    public bool IsClientInputRequired { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }
}
