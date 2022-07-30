using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using cswm.Events;
using cswm.WinApi;
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
        _trayService.AddToSystemTray();
        _winHookService.Start();
        _wmService.Start();

        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => On_ExitApplicationEvent());
        _bus.Events.Where(@event => @event is WindowEvent)
            .Subscribe(@event => On_WindowEvent((@event as WindowEvent)!));
    }

    private void On_WindowEvent(WindowEvent @event)
    {
        _logger?.LogDebug("Received WindowEvent: {event}.", @event);
    }

    private void On_ExitApplicationEvent()
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");
        _trayService.RemoveFromSystemTray();

        Application.Exit();
    }
}