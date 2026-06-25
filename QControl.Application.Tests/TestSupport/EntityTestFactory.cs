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
        WaitingArea? waitingArea = null,
        string? descriptiveName = null,
        string? ipAddress = null,
        bool enableTicketBooking = false,
        bool enableDirectCall = false,
        bool isDeleted = false,
        DateTime? deletedOnUtc = null)
    {
        var window = QControl.Domain.Entities.Window.Create(
            waitingAreaId,
            number,
            descriptiveName: descriptiveName,
            ipAddress: ipAddress,
            enableTicketBooking: enableTicketBooking,
            enableDirectCall: enableDirectCall,
            createdByApplicationUserId: CurrentUserId);

        SetId(window, id);

        if (isDeleted)
        {
            window.IsDeleted = true;
            window.DeletedOnUtc = deletedOnUtc ?? DateTime.UtcNow;
        }

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

    public static Terminal Terminal(
        int id,
        int windowId,
        string number = "T-01",
        string ipAddress = "10.10.0.1",
        string serialNo = "SERIAL-001",
        string type = "Operator Module",
        Window? window = null,
        bool isDeleted = false,
        DateTime? deletedOnUtc = null)
    {
        var terminal = QControl.Domain.Entities.Terminal.Create(
            windowId,
            number,
            ipAddress,
            serialNo,
            type,
            createdByApplicationUserId: CurrentUserId);

        SetId(terminal, id);

        if (isDeleted)
        {
            terminal.IsDeleted = true;
            terminal.DeletedOnUtc = deletedOnUtc ?? DateTime.UtcNow;
        }

        if (window is not null)
        {
            SetPrivateProperty(
                terminal,
                nameof(QControl.Domain.Entities.Terminal.Window),
                window);
            AddTerminal(window, terminal);
        }

        return terminal;
    }

    public static Display Display(
        int id,
        int branchId,
        int number = 1,
        string ipAddress = "10.20.0.1",
        Branch? branch = null)
    {
        var display = QControl.Domain.Entities.Display.Create(
            branchId,
            number,
            ipAddress,
            serialNo: null,
            type: null,
            createdByApplicationUserId: CurrentUserId);

        SetId(display, id);

        if (branch is not null)
        {
            SetPrivateProperty(
                display,
                nameof(QControl.Domain.Entities.Display.Branch),
                branch);
        }

        return display;
    }

    public static DisplayWindow DisplayWindow(
        int id,
        int displayId,
        int windowId,
        Display? display = null,
        Window? window = null)
    {
        var displayWindow = QControl.Domain.Entities.DisplayWindow.Create(
            displayId,
            windowId,
            CurrentUserId);

        SetId(displayWindow, id);

        if (display is not null)
        {
            SetPrivateProperty(
                displayWindow,
                nameof(QControl.Domain.Entities.DisplayWindow.Display),
                display);
        }

        if (window is not null)
        {
            SetPrivateProperty(
                displayWindow,
                nameof(QControl.Domain.Entities.DisplayWindow.Window),
                window);
            AddDisplayWindow(window, displayWindow);
        }

        return displayWindow;
    }

    public static void AddWindow(WaitingArea waitingArea, Window window)
    {
        var field = typeof(WaitingArea).GetField(
            "_windows",
            BindingFlags.Instance | BindingFlags.NonPublic);

        var windows = (List<Window>)field!.GetValue(waitingArea)!;
        windows.Add(window);
    }

    public static void AddTerminal(Window window, Terminal terminal)
    {
        var field = typeof(Window).GetField(
            "_terminals",
            BindingFlags.Instance | BindingFlags.NonPublic);

        var terminals = (List<Terminal>)field!.GetValue(window)!;
        terminals.Add(terminal);
    }

    public static void AddDisplayWindow(
        Window window,
        DisplayWindow displayWindow)
    {
        var field = typeof(Window).GetField(
            "_displayWindows",
            BindingFlags.Instance | BindingFlags.NonPublic);

        var displayWindows =
            (List<DisplayWindow>)field!.GetValue(window)!;
        displayWindows.Add(displayWindow);
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
