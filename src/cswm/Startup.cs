using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using cswm.Events;
using Microsoft.Extensions.Logging;
using cswm.WindowManagement;

namespace cswm;

internal class Startup
{
    private readonly ILogger? _logger;
    private readonly MessageBus _bus;
    private readonly SystemTrayService _trayService;
    private readonly WinHookService _winHookService;
    private readonly WindowManagementService _wmService;

    public Startup(
        ILogger<Startup> logger,
        MessageBus bus,
        SystemTrayService trayService,
        WinHookService winHookService,
        WindowManagementService windowManagementService)
    {
        _logger = logger;
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new ArgumentNullException(nameof(trayService));
        _winHookService = winHookService ?? throw new ArgumentNullException(nameof(winHookService));
        _wmService = windowManagementService ?? throw new ArgumentNullException(nameof(windowManagementService));
    }

    public void Start()
    {
        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => On_ExitApplicationEvent());

        _trayService.AddToSystemTray();
        _winHookService.Start();
        _wmService.Start();

        _bus.Publish(new ResetTrackedWindowsEvent());
    }

    private void On_ExitApplicationEvent()
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");
        _wmService.Stop();
        _trayService.RemoveFromSystemTray();

        Application.Exit();
    }
}