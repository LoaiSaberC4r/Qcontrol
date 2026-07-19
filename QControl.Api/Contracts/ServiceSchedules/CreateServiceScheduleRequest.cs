namespace Qcontrol.Api.Contracts.ServiceSchedules;

public sealed class CreateServiceScheduleRequest
{
    public IReadOnlyCollection<ServiceScheduleDayRequest> Days
        { get; init; } = Array.Empty<ServiceScheduleDayRequest>();

    public bool IsSlotCodeRequired { get; init; }

    public string? SlotCode { get; init; }
}
