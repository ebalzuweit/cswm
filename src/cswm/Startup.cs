using cswm.Events;
using cswm.WindowManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;

namespace cswm;

internal class Startup
{
    const string APPLICATION_GUID = "bdfadba0-9dda-4374-88ab-968c6eb2efde";

    private readonly ILogger? _logger;
    private readonly MessageBus _bus;
    private readonly SystemTrayService _trayService;

    private Mutex? _applicationMutex;

    public Startup(
        ILogger<Startup> logger,
        MessageBus bus,
        SystemTrayService trayService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trayService);

        _logger = logger;
        _bus = bus;
        _trayService = trayService;
    }

    public void Start()
    {
        _applicationMutex = new Mutex(true, $"Global\\{APPLICATION_GUID}", out var mutexAcquired);
        if (mutexAcquired == false)
        {
            _logger?.LogError("Application already running, exiting.");
            return;
        }

        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => On_ExitApplicationEvent());

        _trayService.Start();

        // message loop - prevents DI container from disposing our services
        Application.Run();
    }

    private void On_ExitApplicationEvent()
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");
        _trayService.Stop();

        Application.Exit();

        _applicationMutex?.Dispose();
    }
}