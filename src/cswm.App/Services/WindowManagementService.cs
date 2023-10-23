using System;

namespace cswm.App.Services;

/// <summary>
/// Main service, manages the other services.
/// </summary>
public class WindowManagementService : IService
{
    private readonly WindowEventRelayService _winEventRelayService;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowArrangementService _layoutService;
    private readonly SystemTrayService _trayService;

    public WindowManagementService(
        WindowEventRelayService winHookService,
        WindowTrackingService trackingService,
        WindowArrangementService layoutService,
        SystemTrayService trayService
    )
    {
        ArgumentNullException.ThrowIfNull(winHookService);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(trayService);

        _winEventRelayService = winHookService;
        _trackingService = trackingService;
        _layoutService = layoutService;
        _trayService = trayService;
    }

    public void Start()
    {
        _trackingService.Start();
        _layoutService.Start();
        _trayService.Start();
        _winEventRelayService.Start();
    }

    public void Stop()
    {
        _winEventRelayService.Stop();
        _trayService.Stop();
        _layoutService.Stop();
        _trackingService.Stop();
    }
}
