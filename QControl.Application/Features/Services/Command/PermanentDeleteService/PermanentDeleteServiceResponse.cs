namespace Qcontrol.Application.Features.Services.Command.PermanentDeleteService;

public sealed record PermanentDeleteServiceResponse
{
    public int ServiceId { get; init; }

    public string Message { get; init; } = string.Empty;
}
