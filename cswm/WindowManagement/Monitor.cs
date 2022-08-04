using System;
using cswm.WinApi;

namespace cswm.WindowManagement;

public class Monitor
{
    public IntPtr hMonitor { get; init; }
    public Rect Position { get; init; }
    public Rect WorkArea { get; init; }
    public string DeviceName { get; init; }

    public Monitor(IntPtr hMonitor)
    {
        this.hMonitor = hMonitor;
        var monitorInfoEx = MonitorInfoEx.Create();
        if (User32.GetMonitorInfo(hMonitor, ref monitorInfoEx))
        {
            Position = monitorInfoEx.rcMonitor;
            WorkArea = monitorInfoEx.rcWork;
            DeviceName = monitorInfoEx.szDevice ?? string.Empty;
        }
    }

    public override string ToString()
    {
        return $"[[{hMonitor}]] {DeviceName} @ {Position}";
    }
}