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
        bool horizontalSplit = rect.Width >= rect.Height;
        var dimension = horizontalSplit ? rect.Width : rect.Height;
        var midpoint = dimension / 2;
        // if the space is split on an odd # of pixels, give the extra to the left partition
        var oddDimension = dimension % 2 == 1;
        if (oddDimension)
            midpoint += 1;
        return horizontalSplit
            ? (
                left: new Rect(rect.Left, rect.Top, rect.Left + midpoint, rect.Bottom),
                right: new Rect(rect.Left + midpoint, rect.Top, rect.Right, rect.Bottom)
            )
            : (
                left: new Rect(rect.Left, rect.Top, rect.Right, rect.Top + midpoint),
                right: new Rect(rect.Left, rect.Top + midpoint, rect.Right, rect.Bottom)
            );
    }
}