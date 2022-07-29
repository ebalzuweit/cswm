using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using cswm.Events;
using cswm.WinApi;

namespace cswm;

internal class Startup
{
    private readonly MessageBus _bus;
    private readonly SystemTrayService _trayService;
    private readonly WinHookService _winHookService;

    public Startup(MessageBus bus, SystemTrayService trayService, WinHookService winHookService)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new ArgumentNullException(nameof(trayService));
        _winHookService = winHookService ?? throw new ArgumentNullException(nameof(winHookService));
    }

    public void Run()
    {
        _trayService.AddToSystemTray();
        _winHookService.Start();

        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => Exit());
    }

    private void Exit()
    {
        _trayService.RemoveFromSystemTray();

        Application.Exit();
    }
}