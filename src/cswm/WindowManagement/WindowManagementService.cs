using System;
using System.Linq;
using System.Text;
using cswm.Events;
using cswm.WindowManagement.Arrangement;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;

public class WindowManagementService
{
    private readonly ILogger? _logger;
    private readonly WindowTrackingService _windowTrackingService;
    private readonly IArrangementStrategy _arrangementStrategy;

    public WindowManagementService(
        ILogger<WindowManagementService> logger,
        MessageBus bus,
        WindowTrackingService windowTrackingService,
        IArrangementStrategy arrangementStrategy)
    {
        _logger = logger;
        _windowTrackingService = windowTrackingService ?? throw new ArgumentNullException(nameof(windowTrackingService));
        _arrangementStrategy = arrangementStrategy ?? throw new ArgumentNullException(nameof(arrangementStrategy));
    }

    public void Start()
    {
        _windowTrackingService.OnTrackedWindowsReset += OnWindowTrackingReset;
        _windowTrackingService.OnWindowTrackingStart += OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop += OnWindowTrackingStop;
        _windowTrackingService.OnWindowMoved += OnWindowMoved;
    }

    public void Stop()
    {
#pragma warning disable CS8601
        _windowTrackingService.OnTrackedWindowsReset -= OnWindowTrackingReset;
        _windowTrackingService.OnWindowTrackingStart -= OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop -= OnWindowTrackingStop;
        _windowTrackingService.OnWindowMoved -= OnWindowMoved;
#pragma warning restore CS8601
    }

    private void UpdateWindowPositions()
    {
        _logger?.LogDebug("Updating window positions");
        var monitors = WinApi.User32.EnumDisplayMonitors()
            .Select(hMonitor => new Monitor(hMonitor))
            .ToArray();
        var windows = _windowTrackingService.Windows;
        // var arrangement = _arrangementStrategy.Arrange(layouts);
        // TODO move windows to positions
    }

    private void OnWindowTrackingReset()
    {
        var windows = _windowTrackingService.Windows;
        var sb = new StringBuilder("Tracked windows:\n");
        foreach (var window in windows)
        {
            sb.AppendLine("\t" + window.ToString());
        }
        _logger?.LogInformation(sb.ToString());
    }

    private void OnWindowTrackingStart(Window window)
    {
        _logger?.LogInformation("Started tracking window: {window}", window);
        UpdateWindowPositions();
    }

    private void OnWindowTrackingStop(Window window)
    {
        _logger?.LogInformation("Stopped tracking window: {window}", window);
        UpdateWindowPositions();
    }

    private void OnWindowMoved(Window window)
    {
        _logger?.LogInformation("Window moved: {window}", window);
        UpdateWindowPositions();
    }
}