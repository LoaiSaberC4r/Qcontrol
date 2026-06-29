using BuildingBlock.Application.Time;

namespace QControl.Application.Tests.TestSupport;

internal sealed class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; init; } =
        new(2026, 6, 28, 12, 0, 0, DateTimeKind.Utc);
}
