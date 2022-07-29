using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using cswm.Events;

namespace cswm;

internal class Startup
{
    private readonly MessageBus _bus;
    private readonly SystemTrayService _trayService;

    public Startup(MessageBus bus, SystemTrayService trayService)
    {
        _bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new System.ArgumentNullException(nameof(trayService));
    }

    public void Run()
    {
        _trayService.Start();

        _bus.Events.Where(@event => @event is ExitApplicationEvent)
            .Subscribe(_ => Exit());
    }

    private void Exit()
    {
        _trayService.Stop();

        Application.Exit();
    }
}