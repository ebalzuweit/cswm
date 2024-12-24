using System;
using System.Collections.Generic;
using System.Linq;

namespace cswm.Core.Layout.Engines;

public sealed class BspTilingLayoutEngine : ILayoutEngine
{
	/// <inheritdoc/>
	public LayoutResult CalculateLayout(Bounds displayArea, IReadOnlyList<WindowInfo> windows)
	{
		if (windows.Any() == false)
		{
			return new([]);
		}
		else if (windows.Count == 1)
		{
			return new([
				new WindowLayout(windows.First().Handle, displayArea)
			]);
		}
		else if (windows.Count == 2)
		{
			var areas = SplitBounds(displayArea);
			// TODO: area assignment
			return new([
				new WindowLayout(windows.First().Handle, areas.Left),
				new WindowLayout(windows.Last().Handle, areas.Right)
			]);
		}
		else
		{
			throw new NotImplementedException("TODO: Implement >2 window layout logic.");
		}
	}

	public bool ValidateLayout(Bounds displayArea, LayoutResult layout)
	{
		// TODO: For now, always recalculate layouts
		return false;
	}

	private (Bounds Left, Bounds Right) SplitBounds(Bounds b)
	{
		int split;
		if (b.Width >= b.Height)
		{
			split = b.Left + (b.Width / 2);
			return (
				new Bounds(b.Left, b.Top, split, b.Bottom),
				new Bounds(split + 1, b.Top, b.Right, b.Bottom)
			);
		}
		else
		{
			split = b.Top + (b.Height / 2);
			return (
				new Bounds(b.Left, b.Top, b.Right, split),
				new Bounds(b.Left, split + 1, b.Right, b.Bottom)
			);
		}
	}
}