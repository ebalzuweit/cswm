using cswm.Arrangement.Partitioning;
using cswm.Options;
using cswm.WinApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace cswm.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Split";

    private readonly WindowManagementOptions _options;
    private readonly ILogger<SplitArrangementStrategy> _logger;
    private readonly Dictionary<IntPtr, BspSpace> _spaceCache = new();
    private MonitorLayout _lastArrangement = null!;

    public SplitArrangementStrategy(IOptions<WindowManagementOptions> options, ILogger<SplitArrangementStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public MonitorLayout Arrange(MonitorLayout layout)
    {
        if (layout.Windows.Any() == false)
        {
            return layout;
        }

        _lastArrangement = UpdateLayout_ForGenericWindowEvent(layout);
        return _lastArrangement;
    }

    /// <inheritdoc/>
    public MonitorLayout ArrangeOnWindowMove(MonitorLayout layout, Window movedWindow, Point cursorPosition)
    {
        if (layout.Windows.Any() == false)
        {
            return layout;
        }

        var prev = _lastArrangement.Windows.Where(w => w.Window.hWnd == movedWindow.hWnd).FirstOrDefault();
        if (prev is not default(WindowLayout))
        {
            var wasResize = DetectWindowResize(prev.Position, movedWindow.Position);
            _lastArrangement = wasResize
                ? UpdateLayout_ForWindowResized(layout, movedWindow)
                : UpdateLayout_ForWindowMoved(layout, movedWindow, cursorPosition);
        }
        else
        {
            _lastArrangement = UpdateLayout_ForGenericWindowEvent(layout);
        }

        return _lastArrangement;

        static bool DetectWindowResize(Rect from, Rect to)
        {
            // Count number of edges that moved
            var c = 0;
            if (from.Left != to.Left)
                c++;
            if (from.Top != to.Top)
                c++;
            if (from.Right != to.Right)
                c++;
            if (from.Bottom != to.Bottom)
                c++;

            if (c == 1 || c == 2)
                return true;
            return false;
        }
    }

    public void Reset()
    {
        _spaceCache.Clear();
    }

    private (BspSpace, bool cacheMiss) GetBspSpace(MonitorLayout layout)
    {
        if (_spaceCache.ContainsKey(layout.Monitor.hMonitor))
        {
            return (_spaceCache[layout.Monitor.hMonitor], false);
        }
        else
        {
            // Create new BspSpace for monitor
            var area = layout.Monitor.WorkArea.AddMargin(_options.MonitorPadding);
            var space = new BspSpace(area);
            space.SetTotalWindowCount(layout.Windows.Count());
            return (space, true);
        }
    }

    private MonitorLayout UpdateLayout_ForGenericWindowEvent(MonitorLayout layout)
    {
        var windowLayouts = layout.Windows.ToList();

        // Partition the monitor
        var (bspSpace, cacheMiss) = GetBspSpace(layout);
        if (cacheMiss == false)
        {
            // Update existing partitions
            var spacesCount = bspSpace.GetSpaces().Count();
            if (spacesCount != windowLayouts.Count())
            {
                bspSpace.SetTotalWindowCount(windowLayouts.Count());
            }
        }
        _spaceCache[layout.Monitor.hMonitor] = bspSpace;

        // Match windows to partitioned space
        var potentialMatches = layout.Windows.SelectMany
            (
                w => bspSpace.GetSpaces(_options.WindowMargin / 2).Select
                (
                    s => new WindowPlacement(w, s)
                )
            ).ToList();
        var acceptedMatches = new List<WindowPlacement>(windowLayouts.Count());
        while (potentialMatches.Any())
        {
            var bestMatch = potentialMatches.MinBy(wp => wp.Score);
            potentialMatches.RemoveAll(wp =>
                wp.WindowLayout.Window.hWnd == bestMatch.WindowLayout.Window.hWnd ||
                wp.Space.Equals(bestMatch.Space));
            acceptedMatches.Add(bestMatch);
        }

        // Build arrangement
        var newWindowLayouts = acceptedMatches.Select(wp =>
        {
            var position = wp.Space.AdjustForWindowsPadding(wp.WindowLayout.Window);
            return new WindowLayout(wp.WindowLayout.Window, position);
        });
        return new MonitorLayout(layout.Monitor, newWindowLayouts);
    }

    private MonitorLayout UpdateLayout_ForWindowMoved(MonitorLayout layout, Window movedWindow, Point cursorPosition)
    {
        var windowLayouts = layout.Windows.ToList();

        // Partition the monitor
        var (bspSpace, cacheMiss) = GetBspSpace(layout);
        if (cacheMiss == false)
        {
            // Update existing partitions
            var spacesCount = bspSpace.GetSpaces().Count();
            if (spacesCount != windowLayouts.Count())
            {
                bspSpace.SetTotalWindowCount(windowLayouts.Count());
            }
        }
        _spaceCache[layout.Monitor.hMonitor] = bspSpace;

        // Match windows to partitioned space
        var potentialMatches = layout.Windows.SelectMany
            (
                w => bspSpace.GetSpaces(_options.WindowMargin / 2).Select
                (
                    s => new WindowPlacement(w, s)
                )
            ).ToList();
        var acceptedMatches = new List<WindowPlacement>(windowLayouts.Count());

        // Place moved window into preferred space
        {
            var movedWindowMatch = potentialMatches.Where(wp =>
                wp.WindowLayout.Window.hWnd == movedWindow.hWnd &&
                IsCursorInSpace(cursorPosition, wp.Space)).FirstOrDefault();
            if (movedWindowMatch != default(WindowPlacement))
            {
                acceptedMatches.Add(movedWindowMatch);
                potentialMatches.RemoveAll(wp =>
                    wp.WindowLayout.Window.hWnd == movedWindowMatch.WindowLayout.Window.hWnd ||
                    wp.Space.Equals(movedWindowMatch.Space));
            }
        }

        while (potentialMatches.Any())
        {
            var bestMatch = potentialMatches.MinBy(wp => wp.Score);
            potentialMatches.RemoveAll(wp =>
                wp.WindowLayout.Window.hWnd == bestMatch.WindowLayout.Window.hWnd ||
                wp.Space.Equals(bestMatch.Space));
            acceptedMatches.Add(bestMatch);
        }

        // Build arrangement
        var newWindowLayouts = acceptedMatches.Select(wp =>
        {
            var position = wp.Space.AdjustForWindowsPadding(wp.WindowLayout.Window);
            return new WindowLayout(wp.WindowLayout.Window, position);
        });
        return new MonitorLayout(layout.Monitor, newWindowLayouts);

        static bool IsCursorInSpace(Point cursor, Rect space)
        {
            if (cursor.X < space.Left ||
                cursor.X > space.Right ||
                cursor.Y < space.Top ||
                cursor.Y > space.Bottom)
                return false;
            return true;
        }
    }

    private MonitorLayout UpdateLayout_ForWindowResized(MonitorLayout layout, Window resizedWindow)
    {
        var windowLayouts = layout.Windows.ToList();

        // Partition the monitor
        var (bspSpace, cacheMiss) = GetBspSpace(layout);
        if (cacheMiss == false)
        {
            // Update existing partitions
            var spacesCount = bspSpace.GetSpaces().Count();
            if (spacesCount != windowLayouts.Count())
            {
                bspSpace.SetTotalWindowCount(windowLayouts.Count());
            }
            else
            {
                var prev = _lastArrangement.Windows.Where(w => w.Window.hWnd == resizedWindow.hWnd).First();
                var success = bspSpace.TryResize(prev.Position, resizedWindow.Position);
                _logger.LogDebug($"BspSpace resize was successful: {success}");
            }
        }
        _spaceCache[layout.Monitor.hMonitor] = bspSpace;

        // Match windows to partitioned space
        var potentialMatches = layout.Windows.SelectMany
            (
                w => bspSpace.GetSpaces(_options.WindowMargin / 2).Select
                (
                    s => new WindowPlacement(w, s)
                )
            ).ToList();
        var acceptedMatches = new List<WindowPlacement>(windowLayouts.Count());

        // Place resized window into preferred space
        {
            var resizedWindowMatch = potentialMatches
                .Where(wp => wp.WindowLayout.Window.hWnd == resizedWindow.hWnd)
                .MinBy(wp => wp.Score);
            if (resizedWindowMatch != default(WindowPlacement))
            {
                acceptedMatches.Add(resizedWindowMatch);
                potentialMatches.RemoveAll(wp =>
                    wp.WindowLayout.Window.hWnd == resizedWindowMatch.WindowLayout.Window.hWnd ||
                    wp.Space.Equals(resizedWindowMatch.Space));
            }
        }

        while (potentialMatches.Any())
        {
            var bestMatch = potentialMatches.MinBy(wp => wp.Score)!;
            potentialMatches.RemoveAll(wp =>
                wp.WindowLayout.Window.hWnd == bestMatch.WindowLayout.Window.hWnd ||
                wp.Space.Equals(bestMatch.Space));
            acceptedMatches.Add(bestMatch);
        }

        // Build arrangement
        var newWindowLayouts = acceptedMatches.Select(wp =>
        {
            var position = wp.Space.AdjustForWindowsPadding(wp.WindowLayout.Window);
            return new WindowLayout(wp.WindowLayout.Window, position);
        });
        return new MonitorLayout(layout.Monitor, newWindowLayouts);
    }

    sealed internal class WindowPlacement
    {
        public WindowLayout WindowLayout { get; init; }
        public Rect Space { get; init; }
        public int Score { get; init; }

        public WindowPlacement(WindowLayout windowLayout, Rect space)
        {
            WindowLayout = windowLayout;
            Space = space;
            Score = GetRectTransformation();
        }

        private int GetRectTransformation()
        {
            var translation = Math.Abs(Space.Left - WindowLayout.Position.Left) + Math.Abs(Space.Top - WindowLayout.Position.Top);
            var scaling = Math.Abs(Space.Width - WindowLayout.Position.Width) + Math.Abs(Space.Height - WindowLayout.Position.Height);
            return translation + scaling;
        }
    }
}