using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Reservations;

public sealed class ReservationDomainTests
{
    private static readonly Guid Performer = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime Now = new(2026, 8, 31, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_is_active_and_has_created_history()
    {
        var reservation = Create();
        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Equal(ReservationHistoryEventType.Created, Assert.Single(reservation.History).EventType);
    }

    [Fact]
    public void Create_stores_normalized_generic_lookup_value()
    {
        var reservation = Reservation.Create(1, 10, 3, 8,
            Now.AddHours(1), new DateOnly(2026, 8, 31), Now, Performer,
            "  REF-42  ");

        Assert.Equal("REF-42", reservation.LookupValue);
    }

    [Fact]
    public void Cancel_is_terminal_and_keeps_reason()
    {
        var reservation = Create();
        Assert.True(reservation.TryCancel("changed plans", Performer, Now.AddMinutes(1)));
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal("changed plans", reservation.CancellationReason);
        Assert.False(reservation.TryConvert(Now.AddMinutes(2)));
        Assert.False(reservation.TryMarkNoShow(Now.AddMinutes(2)));
    }

    [Fact]
    public void No_show_can_convert_but_expired_cannot()
    {
        var convertible = Create();
        Assert.True(convertible.TryMarkNoShow(Now.AddHours(2)));
        Assert.True(convertible.TryConvert(Now.AddHours(3)));
        Assert.Equal(ReservationStatus.ConvertedToTicket, convertible.Status);

        var expired = Create();
        Assert.True(expired.TryMarkNoShow(Now.AddHours(2)));
        Assert.True(expired.TryExpire(Now.AddDays(1)));
        Assert.False(expired.TryConvert(Now.AddDays(1)));
    }

    private static Reservation Create() => Reservation.Create(1, 10, 3, 8,
        Now.AddHours(1), new DateOnly(2026, 8, 31), Now, Performer);
}
