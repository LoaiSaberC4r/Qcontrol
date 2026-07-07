using BuildingBlock.Domain.EntitiesHelper;
using BuildingBlock.Domain.Primitive;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Service : AggregateRoot<int>, ISoftDeleteEntity
{
    private readonly List<Service> _children = new();
    private readonly List<ServiceImage> _images = new();

    private Service()
    {
    }

    public int? ParentServiceId { get; private set; }

    public Service? ParentService { get; private set; }

    public IReadOnlyCollection<Service> Children =>
        _children.AsReadOnly();

    public IReadOnlyCollection<ServiceImage> Images =>
        _images.AsReadOnly();

    public string ArabicName { get; private set; } = string.Empty;

    public string EnglishName { get; private set; } = string.Empty;

    public string? ArabicUserMessage { get; private set; }

    public string? EnglishUserMessage { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsTicketIssuable { get; private set; }

    public bool IsClientInputRequired { get; private set; }

    public bool HasReservation { get; private set; }

    public int OrderNo { get; private set; }

    public int Priority { get; private set; }

    public string? RangePrefix { get; private set; }

    public int? RangeStartNumber { get; private set; }

    public int? RangeEndNumber { get; private set; }

    public int? WaitingDuration { get; private set; }

    public int? NoOfTicketCopies { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOnUtc { get; set; }

    public DateTime? RestoredOnUtc { get; set; }

    public static Service Create(
        int? parentServiceId,
        string arabicName,
        string englishName,
        string? arabicUserMessage,
        string? englishUserMessage,
        bool isTicketIssuable,
        bool isClientInputRequired,
        bool hasReservation,
        int orderNo,
        int priority,
        string? rangePrefix,
        int? rangeStartNumber,
        int? rangeEndNumber,
        int? waitingDuration,
        int? noOfTicketCopies,
        Guid createdByApplicationUserId)
    {
        return new Service
        {
            ParentServiceId = parentServiceId,
            ArabicName = NormalizeRequired(arabicName),
            EnglishName = NormalizeRequired(englishName),
            ArabicUserMessage = NormalizeOptional(arabicUserMessage),
            EnglishUserMessage = NormalizeOptional(englishUserMessage),
            IsActive = true,
            IsDeleted = false,
            IsTicketIssuable = isTicketIssuable,
            IsClientInputRequired = isClientInputRequired,
            HasReservation = hasReservation,
            OrderNo = orderNo,
            Priority = priority,
            RangePrefix = NormalizeOptional(rangePrefix),
            RangeStartNumber = rangeStartNumber,
            RangeEndNumber = rangeEndNumber,
            WaitingDuration = waitingDuration,
            NoOfTicketCopies = noOfTicketCopies,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        string arabicName,
        string englishName,
        string? arabicUserMessage,
        string? englishUserMessage,
        bool isTicketIssuable,
        bool isClientInputRequired,
        bool hasReservation,
        int orderNo,
        int priority,
        string? rangePrefix,
        int? rangeStartNumber,
        int? rangeEndNumber,
        int? waitingDuration,
        int? noOfTicketCopies,
        Guid lastModifiedByApplicationUserId)
    {
        ArabicName = NormalizeRequired(arabicName);
        EnglishName = NormalizeRequired(englishName);
        ArabicUserMessage = NormalizeOptional(arabicUserMessage);
        EnglishUserMessage = NormalizeOptional(englishUserMessage);
        IsTicketIssuable = isTicketIssuable;
        IsClientInputRequired = isClientInputRequired;
        HasReservation = hasReservation;
        OrderNo = orderNo;
        Priority = priority;
        RangePrefix = NormalizeOptional(rangePrefix);
        RangeStartNumber = rangeStartNumber;
        RangeEndNumber = rangeEndNumber;
        WaitingDuration = waitingDuration;
        NoOfTicketCopies = noOfTicketCopies;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void ChangeParent(
        int? parentServiceId,
        Guid lastModifiedByApplicationUserId)
    {
        ParentServiceId = parentServiceId;
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void Touch(Guid lastModifiedByApplicationUserId)
    {
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public bool SoftDelete(
        DateTime deletedOnUtc,
        Guid deletedByApplicationUserId)
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedOnUtc = deletedOnUtc;
        LastModifiedByApplicationUserId = deletedByApplicationUserId;

        return true;
    }

    public bool Restore(
        DateTime restoredOnUtc,
        Guid restoredByApplicationUserId)
    {
        if (!IsDeleted)
        {
            return false;
        }

        IsDeleted = false;
        IsActive = true;
        RestoredOnUtc = restoredOnUtc;
        LastModifiedByApplicationUserId = restoredByApplicationUserId;

        return true;
    }

    public bool CanIssueTicket(
        bool hasChildren,
        bool allParentsAreActive)
    {
        return
            !IsDeleted &&
            IsActive &&
            IsTicketIssuable &&
            !hasChildren &&
            allParentsAreActive;
    }

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

}
