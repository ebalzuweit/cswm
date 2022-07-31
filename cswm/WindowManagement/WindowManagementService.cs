using System;
using System.Reactive.Linq;
using System.Text;
using cswm.Events;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;

public class WindowManagementService
{
    private readonly ILogger? _logger;
    private readonly WindowTrackingService _windowTrackingService;

    public WindowManagementService(ILogger<WindowManagementService> logger, MessageBus bus, WindowTrackingService windowTrackingService)
    {
        _logger = logger;
        _windowTrackingService = windowTrackingService ?? throw new ArgumentNullException(nameof(windowTrackingService));
    }

    public void Start()
    {
        _windowTrackingService.OnTrackedWindowsReset += OnWindowTrackingReset;
        _windowTrackingService.OnWindowTrackingStart += OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop += OnWindowTrackingStop;
    }

    public void Stop()
    {
#pragma warning disable CS8601
        _windowTrackingService.OnTrackedWindowsReset -= OnWindowTrackingReset;
        _windowTrackingService.OnWindowTrackingStart -= OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop -= OnWindowTrackingStop;
#pragma warning restore CS8601

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
    }

    private void OnWindowTrackingStop(Window window)
    {
        _logger?.LogInformation("Stopped tracking window: {window}", window);
    }
}