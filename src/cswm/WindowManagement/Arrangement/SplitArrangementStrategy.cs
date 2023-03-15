using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using cswm.WinApi;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
	private readonly ILogger _logger;
	private readonly WindowManagementOptions _options;

    public SplitArrangementStrategy(ILogger<SplitArrangementStrategy> logger, IOptions<WindowManagementOptions> options)
    {
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

	public IEnumerable<WindowLayout> Arrange(IEnumerable<MonitorLayout> layouts)
		=> Arrange_Internal(layouts);

	public IEnumerable<WindowLayout> ArrangeOnWindowMove(IEnumerable<MonitorLayout> layouts, Window movedWindow)
		=> Arrange_Internal(layouts, movedWindow);

	private IEnumerable<WindowLayout> Arrange_Internal(IEnumerable<MonitorLayout> layouts, Window? movedWindow = default)
		=> layouts.SelectMany(layout => PartitionSpace(layout.Space.AddMargin(_options.MonitorPadding), layout.Windows, movedWindow));

	/// <summary>
	/// Recursively partition a space, assigning windows greedily.
	/// </summary>
	/// <param name="space">Remaining space to partition.</param>
	/// <param name="windows">Remaining windows to assign.</param>
	/// <returns>Window assignments.</returns>
	private IEnumerable<WindowLayout> PartitionSpace(Rect space, IEnumerable<WindowLayout> windows, Window? preferredWindow)
	{
		if (windows.Any() == false)
			return Array.Empty<WindowLayout>();
		if (windows.Count() == 1)
		{
			return new[] {
				new WindowLayout(windows.First().hWnd, space.AddMargin(_options.WindowPadding))
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

			var windowList = windows.ToList();

			// preferred window
			if (preferredWindow is not null)
			{
                var layout = windowList.Where(x => x.hWnd == preferredWindow.hWnd).SingleOrDefault();
				if (layout is not null)
				{
                    windowList.Remove(layout);
                    var cursorInLeft = IsCursorInSpace(leftSpace);
                    if (cursorInLeft)
                        windowList.Insert(0, layout);
                    else
                        windowList.Add(layout); // doesn't matter where the preferred window goes in the list
                }                
            }

            var leftPartition = new WindowLayout(windowList.First().hWnd, leftSpace);
            var layouts = PartitionSpace(rightSpace, windowList.Skip(1), preferredWindow);
            layouts = layouts.Prepend(leftPartition);
            return layouts;
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

			_logger.LogDebug($"Comparing x:{x} to y:{y}\nx:{xIntersect}\ty:{yIntersect} -> {result}");

			return result;
        }
    }
}