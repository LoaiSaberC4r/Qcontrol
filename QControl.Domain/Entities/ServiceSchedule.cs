using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class ServiceSchedule : AggregateRoot<int>
{
    private readonly List<ServiceScheduleTimeSlot> _timeSlots = new();

    private ServiceSchedule()
    {
    }

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public bool IsSlotCodeRequired { get; private set; }

    public string? SlotCode { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public IReadOnlyCollection<ServiceScheduleTimeSlot> TimeSlots =>
        _timeSlots.AsReadOnly();

    public static ServiceSchedule Create(
        int branchId,
        int serviceId,
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots,
        bool isSlotCodeRequired,
        string? slotCode,
        Guid createdByApplicationUserId)
    {
        EnsurePositive(branchId, nameof(branchId));
        EnsurePositive(serviceId, nameof(serviceId));
        EnsureUserId(createdByApplicationUserId, nameof(createdByApplicationUserId));

        var replacementTimeSlots = BuildTimeSlots(timeSlots);
        var normalizedSlotCode = NormalizeSlotCode(
            isSlotCodeRequired,
            slotCode);

        var schedule = new ServiceSchedule
        {
            BranchId = branchId,
            ServiceId = serviceId,
            IsSlotCodeRequired = isSlotCodeRequired,
            SlotCode = normalizedSlotCode,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        schedule._timeSlots.AddRange(replacementTimeSlots);

        return schedule;
    }

    public void Update(
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots,
        bool isSlotCodeRequired,
        string? slotCode,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc)
    {
        EnsureUserId(modifiedByApplicationUserId, nameof(modifiedByApplicationUserId));

        var replacementTimeSlots = BuildTimeSlots(timeSlots);
        var normalizedSlotCode = NormalizeSlotCode(
            isSlotCodeRequired,
            slotCode);

        _timeSlots.Clear();
        _timeSlots.AddRange(replacementTimeSlots);
        IsSlotCodeRequired = isSlotCodeRequired;
        SlotCode = normalizedSlotCode;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
        ModifiedOnUtc = modifiedOnUtc;
    }

    public bool IsAvailableAt(
        DayOfWeek dayOfWeek,
        TimeOnly currentTime)
    {
        return _timeSlots.Any(slot =>
            slot.DayOfWeek == dayOfWeek &&
            currentTime >= slot.StartTime &&
            currentTime < slot.EndTime);
    }

    private static IReadOnlyCollection<ServiceScheduleTimeSlot> BuildTimeSlots(
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition> timeSlots)
    {
        if (timeSlots is null || timeSlots.Count == 0)
        {
            throw new ArgumentException(
                "At least one service schedule time slot is required.",
                nameof(timeSlots));
        }

        var orderedDefinitions = timeSlots
            .OrderBy(slot => slot.DayOfWeek)
            .ThenBy(slot => slot.StartTime)
            .ThenBy(slot => slot.EndTime)
            .ToArray();

        foreach (var definition in orderedDefinitions)
        {
            if (!Enum.IsDefined(definition.DayOfWeek))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeSlots),
                    "Every time slot must have a valid day of week.");
            }

            if (definition.StartTime >= definition.EndTime)
            {
                throw new ArgumentException(
                    "Every time slot start time must be earlier than its end time.",
                    nameof(timeSlots));
            }
        }

        if (orderedDefinitions
            .GroupBy(slot => new
            {
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime
            })
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Duplicate time slots are not allowed for the same day.",
                nameof(timeSlots));
        }

        foreach (var daySlots in orderedDefinitions.GroupBy(x => x.DayOfWeek))
        {
            TimeOnly? greatestEndTime = null;

            foreach (var current in daySlots)
            {
                if (greatestEndTime.HasValue &&
                    current.StartTime < greatestEndTime.Value)
                {
                    throw new ArgumentException(
                        "Time slots cannot overlap within the same day.",
                        nameof(timeSlots));
                }

                if (!greatestEndTime.HasValue ||
                    current.EndTime > greatestEndTime.Value)
                {
                    greatestEndTime = current.EndTime;
                }
            }
        }

        return orderedDefinitions
            .Select(ServiceScheduleTimeSlot.Create)
            .ToArray();
    }

    private static string? NormalizeSlotCode(
        bool isSlotCodeRequired,
        string? slotCode)
    {
        return ServiceScheduleSlotCodeNormalizer.Normalize(
            isSlotCodeRequired,
            slotCode);
    }

    private static void EnsurePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"{parameterName} must be greater than zero.");
        }
    }

    private static void EnsureUserId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                $"{parameterName} is required.",
                parameterName);
        }
    }
}

public static class ServiceScheduleSlotCodeNormalizer
{
    public const int MaxLength = 100;

    public static string? Normalize(
        bool isSlotCodeRequired,
        string? slotCode)
    {
        if (string.IsNullOrWhiteSpace(slotCode))
        {
            if (isSlotCodeRequired)
            {
                throw new ArgumentException(
                    "A slot code is required when slot code is required.",
                    nameof(slotCode));
            }

            return null;
        }

        var normalized = slotCode.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slotCode),
                $"Slot code must not exceed {MaxLength} characters.");
        }

        return normalized;
    }
}
