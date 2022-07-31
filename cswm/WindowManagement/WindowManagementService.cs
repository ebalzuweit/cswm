using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;

public class WindowManagementService
{
    private readonly ILogger? _logger;
    private readonly WindowTrackingService _windowTrackingService;

    public WindowManagementService(ILogger<WindowManagementService> logger, WindowTrackingService windowTrackingService)
    {
        _logger = logger;
        _windowTrackingService = windowTrackingService ?? throw new ArgumentNullException(nameof(windowTrackingService));
    }

    public void Start()
    {
        _windowTrackingService.ResetTrackedWindows();
        var windows = _windowTrackingService.Windows;
        var sb = new StringBuilder("Tracking windows:\n");
        foreach (var window in windows)
        {
            sb.AppendLine("\t" + window.ToString());
        }
        _logger?.LogInformation(sb.ToString());

        _windowTrackingService.OnWindowTrackingStart += OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop += OnWindowTrackingStop;
    }

    public void Stop()
    {
#pragma warning disable CS8601
        _windowTrackingService.OnWindowTrackingStart -= OnWindowTrackingStart;
        _windowTrackingService.OnWindowtrackingStop -= OnWindowTrackingStop;
#pragma warning restore CS8601

    }

    private void OnWindowTrackingStart(Window window)
    {
        _logger?.LogDebug("Started tracking window: {window}", window);
    }

    private void OnWindowTrackingStop(Window window)
    {
        _logger?.LogDebug("Stopped tracking window: {window}", window);
    }
}