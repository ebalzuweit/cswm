using System;

namespace cswm.WinApi;

public class Monitor
{
    public IntPtr hMonitor { get; init; }
    public Rect Position { get; init; }
    public virtual Rect WorkArea { get; init; }
    public string? DeviceName { get; init; }

    public bool IsPrimary => Position.Left == 0 && Position.Top == 0;
    public float Ratio => Position.Width / (float)Position.Height;

    public static Monitor FromHmon(IntPtr hMonitor)
    {
        MonitorInfoEx info = MonitorInfoEx.Create();
        User32.GetMonitorInfo(hMonitor, ref info);

        return new Monitor
        {
            hMonitor = hMonitor,
            Position = info.rcMonitor,
            WorkArea = info.rcWork,
            DeviceName = info.szDevice ?? string.Empty
        };
    }

    public override string ToString()
    {
        return $"[[{hMonitor}]] {DeviceName}{(IsPrimary ? "*" : string.Empty)} @ {Position}";
    }
}