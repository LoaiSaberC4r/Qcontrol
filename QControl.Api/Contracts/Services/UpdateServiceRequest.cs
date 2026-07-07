namespace Qcontrol.Api.Contracts.Services;

public sealed class UpdateServiceRequest
{
    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public bool? IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public bool HasReservation { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
