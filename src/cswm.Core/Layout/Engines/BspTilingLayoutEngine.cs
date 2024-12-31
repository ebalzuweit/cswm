using System;
using System.Collections.Generic;
using System.Linq;
using cswm.Core.Models;

namespace cswm.Core.Layout.Engines;

public sealed class BspTilingLayoutEngine : ILayoutEngine
{
	/// <inheritdoc/>
	public LayoutResult CalculateLayout(Rect displayArea, IEnumerable<WindowInfo> windows)
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
			var areas = SplitRect(displayArea);
			// TODO: area assignment
			return new([
				new WindowLayout(windows.First().Handle, areas.Left),
				new WindowLayout(windows.Last().Handle, areas.Right)
			]);
		}
		else
		{
			// Partition & recurse
			var areas = SplitRect(displayArea);
			// TODO: area assignment
			var firstLayout = new WindowLayout(windows.First().Handle, areas.Left);
			var recursiveLayout = CalculateLayout(areas.Right, windows.Skip(1).ToArray());
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
}