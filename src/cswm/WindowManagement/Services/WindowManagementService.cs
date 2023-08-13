using System;

namespace cswm.WindowManagement.Services;

/// <summary>
/// Main service, manages the other services.
/// </summary>
public class WindowManagementService : IService
{
    private readonly WinHookService _winHookService;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowLayoutService _layoutService;
    private readonly SystemTrayService _trayService;

    public WindowManagementService(
        WinHookService winHookService,
        WindowTrackingService trackingService,
        WindowLayoutService layoutService,
        SystemTrayService trayService
    )
    {
        ArgumentNullException.ThrowIfNull(winHookService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(trayService);

        _winHookService = winHookService;
        _trackingService = trackingService;
        _layoutService = layoutService;
        _trayService = trayService;
    }

    public void Start()
    {
        _winHookService.Start();
        _trackingService.Start();
        _layoutService.Start();
        _trayService.Start();
    }

    public void Stop()
    {
        _trayService.Stop();
        _layoutService.Stop();
        _trackingService.Stop();
        _winHookService.Stop();
    }
}
