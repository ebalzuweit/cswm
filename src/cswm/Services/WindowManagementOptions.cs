using System;

namespace cswm.Services;

public class WindowManagementOptions
{
    /// <summary>
    /// If set, windows will be tracked but not managed.
    /// </summary>
    public bool DoNotManage { get; init; }

    /// <summary>
    /// Padding for each monitor.
    /// </summary>
    public int MonitorPadding { get; init; }

    /// <summary>
    /// Margin around all windows.
    /// </summary>
    public int WindowMargin { get; init; }

    /// <summary>
    /// Window classes that will be ignored.
    /// </summary>
    public string[] IgnoredWindowClasses { get; init; } = Array.Empty<string>();
}