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
    private readonly WinHookService _winHookService;
    private readonly WindowManagementService _wmService;

    private Mutex? _applicationMutex;

    public Startup(
        ILogger<Startup> logger,
        MessageBus bus,
        SystemTrayService trayService,
        WinHookService winHookService,
        WindowManagementService wmService)
    {
        ArgumentNullException.ThrowIfNull(wmService);

        _logger = logger;
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new ArgumentNullException(nameof(trayService));
        _winHookService = winHookService ?? throw new ArgumentNullException(nameof(winHookService));
        _wmService = wmService;
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

        _trayService.AddToSystemTray();
        _winHookService.Start();
        _wmService.Start();

        _bus.Publish(new ResetTrackedWindowsEvent());

        // message loop - prevents DI container from disposing our services
        Application.Run();
    }

    private void On_ExitApplicationEvent()
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");
        _wmService.Stop();
        _trayService.RemoveFromSystemTray();

        Application.Exit();

        _applicationMutex?.Dispose();
    }
}