using System.Reflection;
using QControl.Domain.Entities;

namespace QControl.Application.Tests.TestSupport;

internal static class EntityTestFactory
{
    public static readonly Guid CurrentUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static Branch Branch(
        int id,
        string? arabicName = null,
        string? englishName = null)
    {
        var branch = QControl.Domain.Entities.Branch.Create(
            arabicName: arabicName ?? $"Arabic {id}",
            englishName: englishName ?? $"English {id}",
            ipAddress: $"10.0.0.{id}",
            license: null,
            governorate: null,
            city: null,
            area: null,
            address: null,
            longitude: null,
            latitude: null,
            createdByApplicationUserId: CurrentUserId);

        SetId(branch, id);

        return branch;
    }

    public static WaitingArea WaitingArea(
        int id,
        int branchId,
        int number,
        Branch? branch = null,
        string? audioDevice = null,
        string? controlDevice = null,
        string? descriptiveName = null)
    {
        var waitingArea = QControl.Domain.Entities.WaitingArea.Create(
            branchId,
            number,
            audioDevice,
            controlDevice,
            descriptiveName,
            CurrentUserId);

        SetId(waitingArea, id);

        if (branch is not null)
        {
            SetPrivateProperty(
                waitingArea,
                nameof(QControl.Domain.Entities.WaitingArea.Branch),
                branch);
        }

        return waitingArea;
    }

    public static Window Window(
        int id,
        int waitingAreaId,
        string number = "1",
        WaitingArea? waitingArea = null)
    {
        var window = QControl.Domain.Entities.Window.Create(
            waitingAreaId,
            number,
            descriptiveName: null,
            ipAddress: null,
            enableTicketBooking: false,
            enableDirectCall: false,
            createdByApplicationUserId: CurrentUserId);

        SetId(window, id);

        if (waitingArea is not null)
        {
            SetPrivateProperty(
                window,
                nameof(QControl.Domain.Entities.Window.WaitingArea),
                waitingArea);
            AddWindow(waitingArea, window);
        }

        return window;
    }

    public static void AddWindow(WaitingArea waitingArea, Window window)
    {
        var field = typeof(WaitingArea).GetField(
            "_windows",
            BindingFlags.Instance | BindingFlags.NonPublic);

        var windows = (List<Window>)field!.GetValue(waitingArea)!;
        windows.Add(window);
    }

    private static void SetId<TEntity, TId>(TEntity entity, TId id)
        where TEntity : class
    {
        var type = entity.GetType();

        while (type is not null)
        {
            var field = type.GetField(
                "<Id>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is not null)
            {
                field.SetValue(entity, id);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("Entity Id backing field was not found.");
    }

    private static void SetPrivateProperty<TEntity, TValue>(
        TEntity entity,
        string propertyName,
        TValue value)
        where TEntity : class
    {
        var property = entity.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property!.SetValue(entity, value);
    }
}
