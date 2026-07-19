using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class ServiceSchedule : AggregateRoot<int>
{
    private readonly List<ServiceScheduleWorkDay> _workDays = new();

    private ServiceSchedule()
    {
    }

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int ServiceId { get; private set; }

    public Service Service { get; private set; } = null!;

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public bool IsSlotCodeRequired { get; private set; }

    public string? SlotCode { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public IReadOnlyCollection<ServiceScheduleWorkDay> WorkDays =>
        _workDays.AsReadOnly();

    public static ServiceSchedule Create(
        int branchId,
        int serviceId,
        TimeOnly startTime,
        TimeOnly endTime,
        IReadOnlyCollection<DayOfWeek> workDays,
        bool isSlotCodeRequired,
        string? slotCode,
        Guid createdByApplicationUserId)
    {
        EnsurePositive(branchId, nameof(branchId));
        EnsurePositive(serviceId, nameof(serviceId));
        EnsureTimeRange(startTime, endTime);
        EnsureUserId(createdByApplicationUserId, nameof(createdByApplicationUserId));

        var schedule = new ServiceSchedule
        {
            BranchId = branchId,
            ServiceId = serviceId,
            StartTime = startTime,
            EndTime = endTime,
            IsSlotCodeRequired = isSlotCodeRequired,
            SlotCode = NormalizeSlotCode(isSlotCodeRequired, slotCode),
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };

        schedule.ReplaceWorkDays(workDays);

        return schedule;
    }

    public void Update(
        TimeOnly startTime,
        TimeOnly endTime,
        IReadOnlyCollection<DayOfWeek> workDays,
        bool isSlotCodeRequired,
        string? slotCode,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc)
    {
        EnsureTimeRange(startTime, endTime);
        EnsureUserId(modifiedByApplicationUserId, nameof(modifiedByApplicationUserId));

        StartTime = startTime;
        EndTime = endTime;
        IsSlotCodeRequired = isSlotCodeRequired;
        SlotCode = NormalizeSlotCode(isSlotCodeRequired, slotCode);
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
        ModifiedOnUtc = modifiedOnUtc;

        ReplaceWorkDays(workDays);
    }

    public bool IsAvailableAt(
        DayOfWeek dayOfWeek,
        TimeOnly currentTime)
    {
        return _workDays.Any(x => x.DayOfWeek == dayOfWeek) &&
            currentTime >= StartTime &&
            currentTime < EndTime;
    }

    private void ReplaceWorkDays(
        IReadOnlyCollection<DayOfWeek> workDays)
    {
        EnsureValidWorkDays(workDays);

        _workDays.Clear();

        foreach (var workDay in workDays
            .Distinct()
            .OrderBy(x => x))
        {
            _workDays.Add(ServiceScheduleWorkDay.Create(workDay));
        }
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

    private static void EnsureTimeRange(
        TimeOnly startTime,
        TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new ArgumentException(
                "Start time must be earlier than end time.",
                nameof(startTime));
        }
    }

    private static void EnsureValidWorkDays(
        IReadOnlyCollection<DayOfWeek> workDays)
    {
        if (workDays is null || workDays.Count == 0)
        {
            throw new ArgumentException(
                "At least one working day is required.",
                nameof(workDays));
        }

        if (workDays.Any(day => !Enum.IsDefined(day)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(workDays),
                "Every working day must be a valid day of week.");
        }

        if (workDays.Select(day => (int)day).Distinct().Count() !=
            workDays.Count)
        {
            throw new ArgumentException(
                "Working days must be distinct.",
                nameof(workDays));
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
