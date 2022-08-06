using System;
using System.Collections.Generic;
using System.Linq;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
    public IEnumerable<WindowLayout> Arrange(IEnumerable<MonitorLayout> monitorLayouts)
    {
        return monitorLayouts.SelectMany(monitorLayout =>
            PartitionSpace(monitorLayout.Monitor.WorkArea, monitorLayout.Windows));
    }

    private IEnumerable<WindowLayout> PartitionSpace(Rect space, IEnumerable<Window> windows)
    {
        if (windows.Count() == 0)
            return Array.Empty<WindowLayout>();
        if (windows.Count() == 1)
        {
            return new[] {
                new WindowLayout(windows.First(), space)
            };
        }

        var (left, right) = Split(space);
        var layouts = PartitionSpace(right, windows.Skip(1));
        var leftPartition = new WindowLayout(windows.First(), left);
        layouts = layouts.Prepend(leftPartition);
        return layouts;
    }

    private (Rect left, Rect right) Split(Rect rect)
    {
        var width = rect.Width;
        var midpoint = width / 2;
        return (
            left: new Rect(rect.Left, rect.Top, midpoint, rect.Bottom),
            right: new Rect(midpoint, rect.Top, rect.Right, rect.Bottom)
        );
    }
}