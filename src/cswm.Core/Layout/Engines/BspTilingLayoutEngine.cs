using System;
using System.Collections.Generic;
using System.Linq;
using cswm.Core.Models;

namespace cswm.Core.Layout.Engines;

public sealed class BspTilingLayoutEngine : ILayoutEngine
{
	/// <inheritdoc/>
	public LayoutResult CalculateLayout(Rect displayArea, IEnumerable<WindowInfo> windows, nint? priorityWindowHandle = null)
	{
		if (windows.Any() == false)
		{
			// No windows
			return new([]);
		}
		else if (windows.Count() == 1)
		{
			// Single window
			return new([
				new WindowLayout(windows.First().Handle, displayArea)
			]);
		}
		else if (windows.Count() == 2)
		{
			// Single partition
			// TODO: Cleanup needed here
			var (left, right) = SplitRect(displayArea);
			var areas = new List<Rect>(2) { left, right };
			var firstWindow = GetFirstWindowToLayout(windows, priorityWindowHandle);
			var firstWindowSpace = PickSpaceForWindow(firstWindow, areas);
			areas.Remove(firstWindowSpace);
			var lastWindowSpace = areas.First();
			var remainingWindows = windows.ToList();
			remainingWindows.Remove(firstWindow);
			var lastWindow = remainingWindows.First();

			return new([
				new WindowLayout(firstWindow.Handle, firstWindowSpace),
				new WindowLayout(lastWindow.Handle, lastWindowSpace)
			]);
		}
		else
		{
			// Partition & recurse
			// TODO: Cleanup needed here
			var (left, right) = SplitRect(displayArea);
			var areas = new List<Rect>(2) { left, right };
			var firstWindow = GetFirstWindowToLayout(windows, priorityWindowHandle);
			var firstWindowSpace = PickSpaceForWindow(firstWindow, areas);
			areas.Remove(firstWindowSpace);
			var lastWindowSpace = areas.First();
			var remainingWindows = windows.ToList();
			remainingWindows.Remove(firstWindow);

			var firstLayout = new WindowLayout(firstWindow.Handle, firstWindowSpace);
			var recursiveLayout = CalculateLayout(lastWindowSpace, remainingWindows);
			return LayoutResult.Merge(firstLayout, recursiveLayout);
		}
	}

	public bool ValidateLayout(Rect displayArea, LayoutResult layout)
	{
		// TODO: For now, always recalculate layouts
		return false;
	}

	private (Rect Left, Rect Right) SplitRect(Rect b)
	{
		int split;
		if (b.Width >= b.Height)
		{
			split = b.Left + (b.Width / 2);
			return (
				new Rect(b.Left, b.Top, split, b.Bottom),
				new Rect(split + 1, b.Top, b.Right, b.Bottom)
			);
		}
		else
		{
			split = b.Top + (b.Height / 2);
			return (
				new Rect(b.Left, b.Top, b.Right, split),
				new Rect(b.Left, split + 1, b.Right, b.Bottom)
			);
		}
	}

	private WindowInfo GetFirstWindowToLayout(IEnumerable<WindowInfo> windows, nint? priorityWindowHandle)
	{
		if (priorityWindowHandle is not null)
		{
			return windows.First(x => x.Handle == priorityWindowHandle);
		}
		return windows.First();
	}

	private Rect PickSpaceForWindow(WindowInfo window, IEnumerable<Rect> spaces)
	{
		return spaces.OrderBy(x =>
		{
			var translation = Math.Abs(window.Bounds.Left - x.Left) + Math.Abs(window.Bounds.Top - x.Top);
			var resize = Math.Abs(window.Bounds.Width - x.Width) + Math.Abs(window.Bounds.Height - x.Height);

			return translation + resize;
		}).First();
	}
}