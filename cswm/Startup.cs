using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using cswm.Events;
using cswm.WinApi;
using Microsoft.Extensions.Logging;

namespace cswm;

internal class Startup
{
    private readonly ILogger? _logger;
    private readonly MessageBus _bus;
    private readonly SystemTrayService _trayService;
    private readonly WinHookService _winHookService;

    public Startup(ILogger<Startup> logger, MessageBus bus, SystemTrayService trayService, WinHookService winHookService)
    {
        _logger = logger;
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new ArgumentNullException(nameof(trayService));
        _winHookService = winHookService ?? throw new ArgumentNullException(nameof(winHookService));
    }

    public void Start()
    {
        _trayService.AddToSystemTray();
        _winHookService.Start();

        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => On_ExitApplicationEvent());
    }

    private void On_ExitApplicationEvent()
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");
        _trayService.RemoveFromSystemTray();

        Application.Exit();
    }
}