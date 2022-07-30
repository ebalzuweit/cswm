using System;
using System.Threading;
using System.Windows.Forms;
using cswm.Events;
using Microsoft.Extensions.Logging;

namespace cswm.WinApi;

public class WinHookService
{
    private readonly ILogger? _logger;
    private readonly MessageBus _bus;

    public WinHookService(ILogger<WinHookService> logger, MessageBus bus)
    {
        _logger = logger;
        _bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
    }

    public void Start()
    {
        var thread = new Thread(() => SetWinEventHooks())
        {
            Name = "cswmhook"
        };
        thread.Start();
    }

    private void SetWinEventHooks()
    {
        SetWinEventHook(EventConstant.EVENT_SYSTEM_FOREGROUND);
        SetWinEventHook(EventConstant.EVENT_SYSTEM_MINIMIZESTART, EventConstant.EVENT_SYSTEM_MINIMIZEEND);
        SetWinEventHook(EventConstant.EVENT_OBJECT_SHOW, EventConstant.EVENT_OBJECT_HIDE);

        // message loop
        Application.Run();
    }

    private IntPtr SetWinEventHook(EventConstant @event) => SetWinEventHook(@event, @event);

    private IntPtr SetWinEventHook(EventConstant eventMin, EventConstant eventMax) =>
        User32.SetWinEventHook(eventMin, eventMax, IntPtr.Zero, WindowEventHookProc, 0, 0, 0);

    private void WindowEventHookProc(IntPtr hWinEventHook, EventConstant eventType, IntPtr hWnd, ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        var isNonWindowEvent = idChild != 0 || idObject != ObjectIdentifier.OBJID_WINDOW || hWnd == IntPtr.Zero;
        if (isNonWindowEvent)
            return;

        var windowEvent = new WindowEvent(hWnd, eventType);
        _logger?.LogTrace("Publishing event: {event}.", windowEvent);
        _bus.Publish(windowEvent);
    }
}