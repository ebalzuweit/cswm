using cswm.Services.Arrangement;
using cswm.Services.SystemTray;
using cswm.Services.Tracking;
using cswm.Services.WinApi;
using System;

namespace cswm.Services;

/// <summary>
/// Main service, manages the other services.
/// </summary>
public class WindowManagementService : IService
{
    private readonly Win32RelayService _win32RelayService;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowArrangementService _layoutService;
    private readonly SystemTrayService _trayService;

    public WindowManagementService(
        Win32RelayService winHookService,
        WindowTrackingService trackingService,
        WindowArrangementService layoutService,
        SystemTrayService trayService
    )
    {
        ArgumentNullException.ThrowIfNull(winHookService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(trayService);

        _win32RelayService = winHookService;
        _trackingService = trackingService;
        _layoutService = layoutService;
        _trayService = trayService;
    }

    public void Start()
    {
        _trackingService.Start();
        _layoutService.Start();
        _trayService.Start();
        _win32RelayService.Start();
    }

    public void Stop()
    {
        _win32RelayService.Stop();
        _trayService.Stop();
        _layoutService.Stop();
        _trackingService.Stop();
    }
}
