
namespace cswm;

internal class Startup
{
    private readonly SystemTrayService _trayService;

    public Startup(SystemTrayService trayService)
    {
        _trayService = trayService ?? throw new System.ArgumentNullException(nameof(trayService));
    }

    public void Run()
    {
        _trayService.Start();
    }

    public void Exit()
    {
        _trayService.Stop();
    }
}