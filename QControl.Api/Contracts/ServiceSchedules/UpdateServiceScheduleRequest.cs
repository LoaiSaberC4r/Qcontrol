namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class UpdateServiceScheduleRequest
{
    public IReadOnlyCollection<ServiceScheduleDayRequest> Days
        { get; init; } = Array.Empty<ServiceScheduleDayRequest>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
