using cswm.WinApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Split";

    private readonly ILogger _logger;
    private readonly WindowManagementOptions _options;

    public SplitArrangementStrategy(ILogger<SplitArrangementStrategy> logger, IOptions<WindowManagementOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _options = options.Value;
    }

    public MonitorLayout Arrange(MonitorLayout layout) => Arrange_Internal(layout);

    public MonitorLayout ArrangeOnWindowMove(MonitorLayout layout, Window movedWindow, Point cursorPosition) => Arrange_Internal(layout, movedWindow, cursorPosition);

    private MonitorLayout Arrange_Internal(MonitorLayout layout, Window? movedWindow = null, Point? cursorPosition = null)
    {
        var space = layout.Monitor.WorkArea.AddMargin(_options.MonitorPadding);
        var arrangedWindows = PartitionSpace(space, layout.Windows, movedWindow, cursorPosition);

        return layout with { Windows = arrangedWindows };
    }

    /// <summary>
    /// Recursively partition a space, assigning windows greedily.
    /// </summary>
    /// <param name="space">Remaining space to partition.</param>
    /// <param name="windows">Remaining windows to assign.</param>
    /// <returns>Window assignments.</returns>
    private IEnumerable<WindowLayout> PartitionSpace(Rect space, IEnumerable<WindowLayout> windows, Window? preferredWindow, Point? cursorPosition)
    {
        if (windows.Any() == false)
            return Array.Empty<WindowLayout>();
        if (windows.Count() == 1)
        {
            return new[] {
                new WindowLayout(windows.First().Window, space.AddMargin(_options.WindowPadding))
            };
        }

        var windowList = windows.ToList();
        IComparer<WindowLayout> comparer = new WindowLayoutIntersectionComparer(_logger, space);
        windowList.Sort(comparer);
        windowList.Reverse();
        return SplitAndRecurse(space, windowList, preferredWindow);

        IEnumerable<WindowLayout> SplitAndRecurse(Rect space, IEnumerable<WindowLayout> windows, Window? preferredWindow)
        {
            var (left, right, verticalSplit) = space.Split();
            var halfMargin = _options.WindowMargin / 2;
            var leftSpace = verticalSplit switch
            {
                true => left.AddMargin(0, 0, halfMargin, 0),
                false => left.AddMargin(0, 0, 0, halfMargin)
            };
            var rightSpace = verticalSplit switch
            {
                true => right.AddMargin(halfMargin, 0, 0, 0),
                false => right.AddMargin(0, halfMargin, 0, 0)
            };
            _logger.LogDebug("Left: {Left}; Right: {Right}", leftSpace, right);

            var windowList = windows.ToList();

            // preferred window
            if (preferredWindow is not null)
            {
                var layout = windowList.Where(x => x.Window.hWnd == preferredWindow.hWnd).FirstOrDefault();
                if (layout is not null)
                {
                    windowList.Remove(layout);
                    var preferLeft = IsCursorInSpace(leftSpace);
                    if (preferLeft)
                        windowList.Insert(0, layout);
                    else
                        windowList.Add(layout); // doesn't matter where the preferred window goes in the list
                }
            }

            var leftPartition = new WindowLayout(windowList.First().Window, leftSpace.AddMargin(_options.WindowPadding));
            var layouts = PartitionSpace(rightSpace, windowList.Skip(1), preferredWindow, cursorPosition);
            layouts = layouts.Prepend(leftPartition);
            return layouts;
        }

        bool IsCursorInSpace(Rect space)
        {
            if (cursorPosition!.Value.X < space.Left ||
                cursorPosition!.Value.X > space.Right ||
                cursorPosition!.Value.Y < space.Top ||
                cursorPosition!.Value.Y > space.Bottom)
                return false;
            return true;
        }
    }

    private bool IsCursorInSpace(Rect space)
    {
        var point = new Point();
        if (User32.GetCursorPos(ref point))
        {
            _logger.LogDebug("Space: {Space}\tCursor: {Cursor}", space, point);
            if (point.X < space.Left ||
                point.X > space.Right ||
                point.Y < space.Top ||
                point.Y > space.Bottom)
                return false;
            return true;
        }
        return false;
    }

    private class WindowLayoutIntersectionComparer : IComparer<WindowLayout>
    {
        private readonly ILogger _logger;
        private readonly Rect _space;

        public WindowLayoutIntersectionComparer(ILogger logger, Rect space)
        {
            _logger = logger;
            _space = space;
        }

        public int Compare(WindowLayout? x, WindowLayout? y)
        {
            var xn = x is null;
            var yn = y is null;
            if (xn || yn)
            {
                return (xn, yn) switch
                {
                    (true, true) => 0,
                    (true, false) => -1,
                    (false, true) => 1,
                    _ => throw new NotSupportedException() // not possible
                };
            }

            var xIntersect = _space.IntersectionAreaPct(x!.Position);
            var yIntersect = _space.IntersectionAreaPct(y!.Position);
            var result = xIntersect.CompareTo(yIntersect);

#if DEBUG
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Comparing x:{x} to y:{y}\nx:{xIntersect}\ty:{yIntersect} -> {result}");
#endif

            return result;
        }
    }
}