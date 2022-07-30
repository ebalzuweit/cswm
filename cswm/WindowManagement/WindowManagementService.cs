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
        var sb = new StringBuilder();
        foreach (var window in windows)
        {
            sb.AppendLine(window.ToString());
        }
        _logger?.LogInformation(sb.ToString());
    }
}