using System;
using System.Threading;
using System.Windows.Forms;
using cswm.Events;

namespace cswm.WinApi;

public class WinHookService
{
    private readonly MessageBus _bus;

    public WinHookService(MessageBus bus)
    {
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

    public void Stop() { }

    private void SetWinEventHooks()
    {
        User32.SetWinEventHook(EventConstant.EVENT_SYSTEM_FOREGROUND, EventConstant.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, WindowEventHookProc, 0, 0, 0);

        // message loop
        Application.Run();
    }

    private void WindowEventHookProc(IntPtr hWinEventHook, EventConstant eventType, IntPtr hWnd, ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        Console.WriteLine($"Foreground window: {hWnd}");
    }
}