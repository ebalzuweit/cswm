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
        User32.SetWinEventHook(EventConstant.EVENT_SYSTEM_FOREGROUND, EventConstant.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, WindowEventHookProc, 0, 0, 0);

        // message loop
        Application.Run();
    }

    private void WindowEventHookProc(IntPtr hWinEventHook, EventConstant eventType, IntPtr hWnd, ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        _logger?.LogDebug("Publishing event: {eventType} hWnd: {hWnd}", eventType, hWnd);
        _bus.Publish(new ForegroundWindowChangeEvent(hWnd));
    }
}