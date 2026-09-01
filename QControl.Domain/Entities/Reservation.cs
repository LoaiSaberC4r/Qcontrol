using BuildingBlock.Domain.EntitiesHelper;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class Reservation : AggregateRoot<int>
{
    private readonly List<ReservationHistory> _history = new();
    private readonly List<ReservationCustomInputValue> _customInputValues = new();
    private Reservation() { }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;
    public int ServiceId { get; private set; }
    public Service Service { get; private set; } = null!;
    public int SegmentId { get; private set; }
    public Segment Segment { get; private set; } = null!;
    public int BranchServiceSegmentId { get; private set; }
    public BranchServiceSegment BranchServiceSegment { get; private set; } = null!;
    public string? LookupValue { get; private set; }
    public DateTime ScheduledOnUtc { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledOnUtc { get; private set; }
    public Guid? CancelledByApplicationUserId { get; private set; }
    public DateTime? ConvertedToTicketOnUtc { get; private set; }
    public DateTime? ExpiredOnUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    public IReadOnlyCollection<ReservationHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<ReservationCustomInputValue> CustomInputValues => _customInputValues.AsReadOnly();

    public static Reservation Create(int branchId, int serviceId, int segmentId,
        int branchServiceSegmentId, DateTime scheduledOnUtc, DateOnly businessDate,
        DateTime createdOnUtc, Guid performerId, string? lookupValue = null)
    {
        var reservation = new Reservation
        {
            BranchId = branchId,
            ServiceId = serviceId,
            SegmentId = segmentId,
            BranchServiceSegmentId = branchServiceSegmentId,
            LookupValue = string.IsNullOrWhiteSpace(lookupValue) ? null : lookupValue.Trim(),
            ScheduledOnUtc = scheduledOnUtc,
            BusinessDate = businessDate,
            Status = ReservationStatus.Active,
            CreatedOnUtc = createdOnUtc
        };
        reservation.AppendHistory(ReservationHistoryEventType.Created, null,
            ReservationStatus.Active, performerId, null, createdOnUtc);
        return reservation;
    }

    public void AddCustomInput(ReservationCustomInputValue value) =>
        _customInputValues.Add(value ?? throw new ArgumentNullException(nameof(value)));

    public bool TryCancel(string reason, Guid performerId, DateTime occurredOnUtc)
    {
        if (string.IsNullOrWhiteSpace(reason) ||
            Status is not (ReservationStatus.Active or ReservationStatus.NoShow)) return false;
        var from = Status;
        Status = ReservationStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledOnUtc = occurredOnUtc;
        CancelledByApplicationUserId = performerId;
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(ReservationHistoryEventType.Cancelled, from, Status,
            performerId, CancellationReason, occurredOnUtc);
        return true;
    }

    public bool TryMarkNoShow(DateTime occurredOnUtc)
    {
        if (Status != ReservationStatus.Active) return false;
        Status = ReservationStatus.NoShow;
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(ReservationHistoryEventType.NoShow, ReservationStatus.Active,
            Status, null, null, occurredOnUtc);
        return true;
    }

    public bool TryExpire(DateTime occurredOnUtc)
    {
        if (Status != ReservationStatus.NoShow) return false;
        Status = ReservationStatus.Expired;
        ExpiredOnUtc = occurredOnUtc;
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(ReservationHistoryEventType.Expired, ReservationStatus.NoShow,
            Status, null, null, occurredOnUtc);
        return true;
    }

    public bool TryConvert(DateTime occurredOnUtc)
    {
        if (Status is not (ReservationStatus.Active or ReservationStatus.NoShow)) return false;
        var from = Status;
        Status = ReservationStatus.ConvertedToTicket;
        ConvertedToTicketOnUtc = occurredOnUtc;
        ModifiedOnUtc = occurredOnUtc;
        AppendHistory(ReservationHistoryEventType.ConvertedToTicket, from,
            Status, null, null, occurredOnUtc);
        return true;
    }

    private void AppendHistory(ReservationHistoryEventType eventType,
        ReservationStatus? fromStatus, ReservationStatus? toStatus,
        Guid? performerId, string? reason, DateTime occurredOnUtc) =>
        _history.Add(ReservationHistory.Create(eventType, fromStatus, toStatus,
            performerId, reason, occurredOnUtc));
}

public sealed class ReservationHistory : Entity<long>
{
    private ReservationHistory() { }
    public int ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = null!;
    public ReservationHistoryEventType EventType { get; private set; }
    public ReservationStatus? FromStatus { get; private set; }
    public ReservationStatus? ToStatus { get; private set; }
    public Guid? PerformerApplicationUserId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }

    public static ReservationHistory Create(ReservationHistoryEventType eventType,
        ReservationStatus? fromStatus, ReservationStatus? toStatus, Guid? performerId,
        string? reason, DateTime occurredOnUtc) => new()
        {
            EventType = eventType,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            PerformerApplicationUserId = performerId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            OccurredOnUtc = occurredOnUtc,
            CreatedOnUtc = occurredOnUtc
        };
}

public sealed class ReservationCustomInputValue : Entity<int>
{
    private ReservationCustomInputValue() { }
    public int ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = null!;
    public int? ServiceCustomInputId { get; private set; }
    public string NameSnapshot { get; private set; } = string.Empty;
    public string? LabelEnSnapshot { get; private set; }
    public string? LabelArSnapshot { get; private set; }
    public ServiceCustomInputType TypeSnapshot { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public static ReservationCustomInputValue Create(int? inputId, string name,
        string? labelEn, string? labelAr, ServiceCustomInputType type, string value,
        DateTime createdOnUtc) => new()
        {
            ServiceCustomInputId = inputId,
            NameSnapshot = name,
            LabelEnSnapshot = labelEn,
            LabelArSnapshot = labelAr,
            TypeSnapshot = type,
            Value = value,
            CreatedOnUtc = createdOnUtc
        };
}
