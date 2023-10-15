using cswm.Services;
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

    private readonly ILogger _logger;
    private readonly WindowManagementOptions _options;
    private MonitorLayout _lastArrangement;
    private Dictionary<IntPtr, PartitionedSpace> _priorPartitions = new();

    public SplitArrangementStrategy(ILogger<SplitArrangementStrategy> logger, IOptions<WindowManagementOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public MonitorLayout Arrange(MonitorLayout layout) => Arrange_Internal(layout);

    /// <inheritdoc/>
    public MonitorLayout ArrangeOnWindowMove(MonitorLayout layout, Window movedWindow, Point cursorPosition)
        => Arrange_Internal(layout, movedWindow, cursorPosition);

    private MonitorLayout Arrange_Internal(MonitorLayout layout, Window? movedWindow = null, Point? cursorPosition = null)
    {
        var prior = _priorPartitions.GetValueOrDefault(layout.Monitor.hMonitor);
        if (prior is null)
        {
            // Create partitions new
            prior = new PartitionedSpace(layout.Monitor.WorkArea, _options);
            prior.SetTotalWindowCount(layout.Windows.Count());
        }
        else
        {
            // Update existing partitions
            if (prior.GetWindowSpaces().Count == layout.Windows.Count())
            {
                // Keep existing partitions
                if (movedWindow is not null)
                {
                    TryHandleMovedWindowResized(movedWindow);
                }
            }
            else
            {
                // FIXME: Preserve partitions when adding / removing windows
                prior.SetTotalWindowCount(layout.Windows.Count());
            }
        }
        var spaces = prior.GetWindowSpaces();
        _priorPartitions[layout.Monitor.hMonitor] = prior;


        var windowLayouts = layout.Windows.ToList();
        var arrangedWindows = LayoutWindows(windowLayouts, spaces, movedWindow, cursorPosition);

        _lastArrangement = layout with { Windows = arrangedWindows };
        return _lastArrangement;

        // uses local var prior
        bool TryHandleMovedWindowResized(Window moved)
        {
            var prev = _lastArrangement.Windows.Where(w => w.Window.hWnd == moved.hWnd).FirstOrDefault();
            if (prev is not default(WindowLayout))
            {
                var wasResize = DetectWindowResize(prev.Position, moved.Position);
                if (wasResize)
                {
                    prior.ResizeSpace(prev.Position, moved.Position);
                    return true;
                }
            }
            return false;
        }
    }

    private bool DetectWindowResize(Rect from, Rect to)
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

    /// <summary>
    /// Place windows in spaces.
    /// </summary>
    /// <param name="windowLayouts">Current window layouts.</param>
    /// <param name="spaces">New window spaces.</param>
    /// <returns>Updated window layouts.</returns>
    private IEnumerable<WindowLayout> LayoutWindows(IList<WindowLayout> windowLayouts, IList<Rect> spaces, Window? movedWindow = null, Point? cursorPosition = null)
    {
        var arrangement = new List<WindowLayout>(windowLayouts.Count);
        var unassignedWindows = new List<Window>(windowLayouts.Select(x => x.Window));
        var unassignedSpaces = new List<Rect>(spaces);

        if (movedWindow is not null && cursorPosition is not null)
        {
            // Arrange on window move
            var destination = unassignedSpaces.Where(IsCursorInSpace).FirstOrDefault();
            if (unassignedSpaces.Contains(destination)) // check against default
            {
                // Assign moved window to space containing the cursor
                Assign(movedWindow, destination);
            }
        }

        var allArrangements = unassignedWindows.SelectMany(w => unassignedSpaces.Select(s => new WindowLayout(w, s)));
        var sortedArrangements = allArrangements.OrderBy(a =>
        {
            // Sort arrangements by geometric similarity
            var current = windowLayouts.Where(w => w.Window == a.Window).Single();
            var similarity = GetRectSimilarity(a.Position, current.Position);
            return similarity;
        }).ToList();

        while (unassignedSpaces.Any() && unassignedWindows.Any() && sortedArrangements.Any())
        {
            var a = sortedArrangements.First();
            Assign(a.Window, a.Position);
            sortedArrangements.RemoveAll(x => x.Window == a.Window || x.Position.Equals(a.Position));
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
            var position = space.AdjustForWindowsPadding(window);

            unassignedWindows!.Remove(window);
            unassignedSpaces!.Remove(space);
            arrangement!.Add(new(window, position));
        }
    }

    /// <summary>
    /// Get the geometric similarity between two <see cref="Rect"/>.
    /// </summary>
    /// <param name="a">The first Rect to compare.</param>
    /// <param name="b">The second Rect to compare.</param>
    /// <returns><c>0</c> if the <see cref="Rect"/> are equal; otherwise, the total translation and scaling difference between them.</returns>
    private int GetRectSimilarity(Rect a, Rect b)
    {
        var translation = Math.Abs(b.Left - a.Left) + Math.Abs(b.Top - a.Top);
        var scaling = Math.Abs(b.Width - a.Width) + Math.Abs(b.Height - a.Height);

        return translation + scaling;
    }
}