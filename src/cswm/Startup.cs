using cswm.Events;
using cswm.WindowManagement.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Windows.Forms;

namespace cswm;

/// <summary>
/// Application entry-point, hosts <see cref="WindowManagementService"/>.
/// </summary>
internal class Startup
{
    const string APPLICATION_GUID = "bdfadba0-9dda-4374-88ab-968c6eb2efde";

    private readonly ILogger? _logger;
    private readonly MessageBus _bus;
    private readonly WindowManagementService _windowMgmtService;

    private Mutex? _applicationMutex;
    private IDisposable? _subscription;

    public Startup(
        ILogger<Startup> logger,
        MessageBus bus,
        WindowManagementService windowMgmtService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(windowMgmtService);

        _logger = logger;
        _bus = bus;
        _windowMgmtService = windowMgmtService;
    }

    public void Start()
    {
#if !DEBUG
        _applicationMutex = new Mutex(true, $"Global\\{APPLICATION_GUID}", out var mutexAcquired);
        if (mutexAcquired == false)
        {
            _logger?.LogError("Application already running, exiting.");
            return;
        }
#endif

        _subscription = _bus.Subscribe<ExitApplicationEvent>(On_ExitApplicationEvent);
        _windowMgmtService.Start();
        Application.Run(); // message loop - prevents DI container from disposing our services
    }

    private void On_ExitApplicationEvent(ExitApplicationEvent _)
    {
        _logger?.LogInformation("ExitApplicationEvent received, exiting.");

        _applicationMutex?.Dispose();
        _subscription?.Dispose();
        _windowMgmtService.Stop();
        Application.Exit();
    }
}