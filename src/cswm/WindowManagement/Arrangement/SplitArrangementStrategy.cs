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
        var windowLayouts = layout.Windows.ToList();
        var partitions = PartitionSpace(space, windowLayouts.Count);
        var arrangedWindows = LayoutWindows(windowLayouts, partitions, movedWindow, cursorPosition);

        return layout with { Windows = arrangedWindows };
    }

    /// <summary>
    /// Recursively partition a space.
    /// </summary>
    /// <param name="space">Remaining space to partition.</param>
    /// <param name="count">Number of partitions.</param>
    /// <returns></returns>
    private IList<Rect> PartitionSpace(Rect space, int count)
    {
        if (count <= 0)
        {
            return Array.Empty<Rect>();
        }
        else if (count == 1)
        {
            return new[] { space.AddMargin(_options.WindowPadding) };
        }

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

        var partitions = PartitionSpace(rightSpace, count - 1);
        var arr = new Rect[count];
        arr[0] = leftSpace;
        for (int i = 0; i < partitions.Count; i++)
        {
            arr[i + 1] = partitions[i];
        }
        return arr;
    }

    /// <summary>
    /// Place windows in spaces.
    /// </summary>
    /// <param name="windowLayouts">Current window layouts.</param>
    /// <param name="spaces">New window spaces.</param>
    /// <returns>Updated window layouts.</returns>
    private IEnumerable<WindowLayout> LayoutWindows(IList<WindowLayout> windowLayouts, IList<Rect> spaces, Window? movedWindow = null, Point? cursorPosition = null)
    {
        if (windowLayouts.Count != spaces.Count)
            throw new ArgumentException("Window and Space count must be equal.");

        var arrangement = new List<WindowLayout>(windowLayouts.Count);
        var unassignedWindows = new List<Window>(windowLayouts.Select(x => x.Window));
        var unassignedSpaces = new List<Rect>(spaces);

        // Assign moved window to space containing the cursor
        if (movedWindow is not null && cursorPosition is not null)
        {
            var destination = unassignedSpaces.Where(IsCursorInSpace).FirstOrDefault();
            var source = windowLayouts.Where(x => x.Window == movedWindow).Select(x => x.Position).FirstOrDefault();
            if (unassignedSpaces.Contains(destination))
            {
                // Assign moved windows to space containing the cursor
                Assign(movedWindow, destination);

                var swapWindow = windowLayouts.Where(x => x.Position.Equals(destination)).Select(x => x.Window).FirstOrDefault();
                if (unassignedSpaces.Contains(source) && swapWindow is not null && unassignedWindows.Contains(swapWindow))
                {
                    // Assign window at destination to moved window's prior space
                    Assign(swapWindow, source);
                }
            }
        }

        // Preserve current window positions if possible
        var preservedArrangements = windowLayouts
            .Where(x => unassignedWindows.Contains(x.Window) && unassignedSpaces.Contains(x.Position))
            .ToList();
        foreach (var pa in preservedArrangements)
        {
            if (unassignedWindows.Contains(pa.Window) &&
                unassignedSpaces.Contains(pa.Position))
                Assign(pa.Window, pa.Position);
        }

        // Assign the remaining windows to the remaining spaces
        for (int i = unassignedWindows.Count - 1; i >= 0; i--)
        {
            var window = unassignedWindows[i];
            var space = unassignedSpaces.First();

            Assign(window, space);
        }

        return arrangement;

        bool IsCursorInSpace(Rect space)
        {
            if (cursorPosition!.Value.X < space.Left ||
                cursorPosition!.Value.X > space.Right ||
                cursorPosition!.Value.Y < space.Top ||
                cursorPosition!.Value.Y > space.Bottom)
                return false;
            return true;
        }

        void Assign(Window window, Rect space)
        {
            unassignedWindows!.Remove(window);
            unassignedSpaces!.Remove(space);
            arrangement!.Add(new(window, space));
        }
    }

    /// <summary>
    /// Recursively partition a space, assigning windows greedily.
    /// </summary>
    /// <param name="space">Remaining space to partition.</param>
    /// <param name="windows">Remaining windows to assign.</param>
    /// <returns>Window assignments.</returns>
    private IEnumerable<WindowLayout> PartitionSpace_Old(Rect space, IEnumerable<WindowLayout> windows, Window? preferredWindow, Point? cursorPosition)
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
            var layouts = PartitionSpace_Old(rightSpace, windowList.Skip(1), preferredWindow, cursorPosition);
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