using System;

namespace cswm.App.Services;

/// <summary>
/// Main service, manages the other services.
/// </summary>
public class WindowManagementService : IService
{
    private readonly WindowEventRelayService _winEventRelayService;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowArrangementService _arrangementService;
    private readonly SystemTrayService _trayService;

    public WindowManagementService(
        WindowEventRelayService winEventRelay,
        WindowTrackingService trackingService,
        WindowArrangementService arrangementService,
        SystemTrayService trayService
    )
    {
        ArgumentNullException.ThrowIfNull(winEventRelay);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(arrangementService);
        ArgumentNullException.ThrowIfNull(trayService);

        _winEventRelayService = winEventRelay;
        _trackingService = trackingService;
        _arrangementService = arrangementService;
        _trayService = trayService;
    }

    public void Start()
    {
        _trackingService.Start();
        _arrangementService.Start();
        _trayService.Start();
        _winEventRelayService.Start();
    }

    public void Stop()
    {
        _winEventRelayService.Stop();
        _trayService.Stop();
        _arrangementService.Stop();
        _trackingService.Stop();
    }
}
