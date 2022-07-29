
using System.Windows.Forms;
using cswm.Events;

namespace cswm;

internal class Startup
{
    private readonly IEventBus _bus;
    private readonly SystemTrayService _trayService;

    public Startup(IEventBus bus, SystemTrayService trayService)
    {
        _bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
        _trayService = trayService ?? throw new System.ArgumentNullException(nameof(trayService));
    }

    public void Run()
    {
        _trayService.Start();

        _bus.Subscribe<ExitApplicationEvent>((@event) => Exit());
    }

    public void Exit()
    {
        _trayService.Stop();

        Application.Exit();
    }
}