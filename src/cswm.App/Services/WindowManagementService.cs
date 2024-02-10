using cswm.Events;
using System;

namespace cswm.App.Services;

/// <summary>
/// Main service, manages the other services.
/// </summary>
public class WindowManagementService : IService
{
    private readonly MessageBus _bus;
    private readonly WindowEventRelayService _winEventRelayService;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowArrangementService _arrangementService;
    private readonly SystemTrayService _trayService;

    public WindowManagementService(
        MessageBus messageBus,
        WindowEventRelayService winEventRelay,
        WindowTrackingService trackingService,
        WindowArrangementService arrangementService,
        SystemTrayService trayService
    )
    {
        ArgumentNullException.ThrowIfNull(messageBus);
        ArgumentNullException.ThrowIfNull(winEventRelay);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(arrangementService);
        ArgumentNullException.ThrowIfNull(trayService);

        _bus = messageBus;
        _winEventRelayService = winEventRelay;
        _trackingService = trackingService;
        _arrangementService = arrangementService;
        _trayService = trayService;
    }

    public void Start()
    {
        // Start the tracking service
        _trackingService.Start();
        // Start the arrangement service - this will trigger initial arrangement
        _arrangementService.Start();
        // Start tracking OS events
        _winEventRelayService.Start();
        // Start the tray application
        _trayService.Start();
    }

    public void Stop()
    {
        _trayService.Stop();
        _winEventRelayService.Stop();
        _trackingService.Stop();
        _arrangementService.Stop();
    }
}
