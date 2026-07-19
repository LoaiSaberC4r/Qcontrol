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
            governorate: $"Governorate {id}",
            city: $"City {id}",
            area: $"Area {id}",
            address: $"Address {id}",
            latitude: id,
            longitude: id,
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
        bool isInactive = false,
        DateTime? deactivatedOnUtc = null)
    {
        var window = QControl.Domain.Entities.Window.Create(
            waitingArea?.BranchId ?? waitingAreaId,
            waitingAreaId,
            number,
            descriptiveName: descriptiveName,
            ipAddress: ipAddress,
            enableTicketBooking: enableTicketBooking,
            enableDirectCall: enableDirectCall,
            createdByApplicationUserId: CurrentUserId);

        SetId(window, id);

        if (isInactive)
        {
            window.Deactivate(
                deactivatedOnUtc ?? DateTime.UtcNow,
                CurrentUserId);
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
        bool isInactive = false,
        DateTime? deactivatedOnUtc = null)
    {
        var terminal = QControl.Domain.Entities.Terminal.Create(
            window?.BranchId ?? windowId,
            windowId,
            number,
            ipAddress,
            serialNo,
            type,
            createdByApplicationUserId: CurrentUserId);

        SetId(terminal, id);

        if (isInactive)
        {
            terminal.Deactivate(
                deactivatedOnUtc ?? DateTime.UtcNow,
                CurrentUserId);
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
        string number = "D-01",
        string ipAddress = "10.20.0.1",
        string serialNo = "DISPLAY-SERIAL-001",
        string type = "LED Display",
        Branch? branch = null,
        bool isInactive = false,
        DateTime? deactivatedOnUtc = null)
    {
        var display = QControl.Domain.Entities.Display.Create(
            branchId,
            number,
            ipAddress,
            serialNo,
            type,
            createdByApplicationUserId: CurrentUserId);

        SetId(display, id);

        if (isInactive)
        {
            display.Deactivate(
                deactivatedOnUtc ?? DateTime.UtcNow,
                CurrentUserId);
        }

        if (branch is not null)
        {
            SetPrivateProperty(
                display,
                nameof(QControl.Domain.Entities.Display.Branch),
                branch);
        }

        return display;
    }

    public static Service Service(
        int id,
        int? parentServiceId = null,
        string? arabicName = null,
        string? englishName = null,
        bool isTicketIssuable = false,
        string? serviceCode = null,
        bool isServiceCodeRequired = false)
    {
        var service = QControl.Domain.Entities.Service.Create(
            parentServiceId,
            arabicName ?? $"Arabic Service {id}",
            englishName ?? $"English Service {id}",
            serviceCode,
            isServiceCodeRequired,
            arabicUserMessage: null,
            englishUserMessage: null,
            isTicketIssuable,
            isClientInputRequired: false,
            hasReservation: false,
            orderNo: id,
            priority: 0,
            rangePrefix: $"S{id}",
            rangeStartNumber: 1,
            rangeEndNumber: 999,
            waitingDuration: 0,
            noOfTicketCopies: 1,
            createdByApplicationUserId: CurrentUserId);

        SetId(service, id);

        return service;
    }

    public static void SetServiceActive(
        Service service,
        bool isActive)
    {
        SetPrivateProperty(
            service,
            nameof(QControl.Domain.Entities.Service.IsActive),
            isActive);
    }

    public static BranchService BranchService(
        int id,
        int branchId,
        int serviceId)
    {
        var branchService = QControl.Domain.Entities.BranchService.Create(
            branchId,
            serviceId,
            CurrentUserId);

        SetId(branchService, id);

        return branchService;
    }

    public static ServiceSchedule ServiceSchedule(
        int id,
        int branchId,
        int serviceId,
        string? slotCode = null,
        bool isSlotCodeRequired = false,
        IReadOnlyCollection<ServiceScheduleTimeSlotDefinition>? timeSlots = null)
    {
        var schedule = QControl.Domain.Entities.ServiceSchedule.Create(
            branchId,
            serviceId,
            timeSlots ?? new[]
            {
                new ServiceScheduleTimeSlotDefinition(
                    DayOfWeek.Sunday,
                    new TimeOnly(8, 0),
                    new TimeOnly(16, 0))
            },
            isSlotCodeRequired,
            slotCode,
            CurrentUserId);

        SetId(schedule, id);

        return schedule;
    }

    public static DisplayWindow DisplayWindow(
        int id,
        int displayId,
        int windowId,
        Display? display = null,
        Window? window = null)
    {
        var displayWindow = QControl.Domain.Entities.DisplayWindow.Create(
            display?.BranchId ?? window?.BranchId ?? 1,
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
