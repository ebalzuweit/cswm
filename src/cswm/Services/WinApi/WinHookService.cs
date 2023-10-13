using cswm.Events;
using cswm.WinApi;
using cswm.WinApi.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace cswm.Services.WinApi;

/// <summary>
/// Intercept and relay Win32 events.
/// </summary>
public class WinHookService : IService
{
    private readonly ILogger? _logger;
    private readonly List<WINEVENTPROC> _hooks = new();
    private readonly MessageBus _bus;

    private Thread? _thread;

    public WinHookService(ILogger<WinHookService> logger, MessageBus bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        _logger = logger;
        _bus = bus;
    }

    public void Start()
    {
        _thread = new Thread(() => SetWinEventHooks())
        {
            Name = "cswmhook"
        };
        _thread.Start();
    }

    public void Stop()
    {
        _thread = null;
    }

    private void SetWinEventHooks()
    {
        SetWinEventHook(EventConstant.EVENT_SYSTEM_MOVESIZEEND);
        SetWinEventHook(EventConstant.EVENT_OBJECT_LOCATIONCHANGE);
        SetWinEventHook(EventConstant.EVENT_SYSTEM_MINIMIZESTART, EventConstant.EVENT_SYSTEM_MINIMIZEEND);
        SetWinEventHook(EventConstant.EVENT_OBJECT_SHOW, EventConstant.EVENT_OBJECT_HIDE);

        // message loop
        Application.Run();
    }

    private HWINEVENTHOOK SetWinEventHook(EventConstant @event) => SetWinEventHook(@event, @event);

    private HWINEVENTHOOK SetWinEventHook(EventConstant eventMin, EventConstant eventMax)
    {
        // hold a reference to each hook delegate to prevent garbage collection
        var hook = new WINEVENTPROC(WindowEventHookProc);
        _hooks.Add(hook);
        return Windows.Win32.PInvoke.SetWinEventHook((uint)eventMin, (uint)eventMax, new HINSTANCE(), hook, 0, 0, 0);
    }

    private void WindowEventHookProc(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        var isNonWindowEvent = idChild != 0 || idObject != (int)Windows.Win32.UI.WindowsAndMessaging.OBJECT_IDENTIFIER.OBJID_WINDOW || hwnd.Value == IntPtr.Zero;
        if (isNonWindowEvent)
            return;

        var windowEvent = new WindowEvent(hwnd, (EventConstant)@event);
        _logger?.LogTrace("Publishing event: {event}.", windowEvent);
        _bus.Publish(windowEvent);
    }
}