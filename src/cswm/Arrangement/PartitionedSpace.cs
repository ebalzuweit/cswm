using System;
using System.Collections.Generic;
using cswm.Services;
using cswm.WinApi;

namespace cswm.Arrangement;

public sealed class PartitionedSpace
{
	private readonly Rect _space;
	private List<Partition> _partitions = new();

	public PartitionedSpace(Rect space)
	{
		_space = space;
	}

	public void SetTotalWindowCount(int spacesCount)
	{
		var partitionCount = Math.Max(0, spacesCount - 1);
		_partitions.Clear();

		var s = _space;
		var verticalSplit = true;
		for (var i = 0; i < partitionCount; i++)
		{
			var dimension = verticalSplit ? s.Width : s.Height;
			var midpoint = dimension / 2;
			if (dimension % 2 == 1)
				midpoint += 1; // left split gets the extra
			_partitions.Add(new(verticalSplit, midpoint));

			(_, s) = s.SplitAt(verticalSplit, midpoint);
			verticalSplit = !verticalSplit;
		}
	}

	// TODO: cache these when setting up partitions
	public IList<Rect> GetWindowSpaces(WindowManagementOptions options)
	{
		var s = _space.AddMargin(options.MonitorPadding);
		if (_partitions.Count == 0)
		{
			// Shortcut if there's no partitions
			return new[] { s };
		}

		var halfMargin = options.WindowMargin / 2;
		var spaces = new List<Rect>();
		foreach (var p in _partitions)
		{
			(var l, var r) = s.SplitAt(p.Vertical, p.Position);
			if (p.Vertical)
			{
				l = l.AddMargin(0, 0, halfMargin, 0); // left
				r = r.AddMargin(halfMargin, 0, 0, 0); // right
			}
			else
			{
				l = l.AddMargin(0, 0, 0, halfMargin); // top
				r = r.AddMargin(0, halfMargin, 0, 0); // bottom
			}
			spaces.Add(l);

			s = r;
		}
		spaces.Add(s);
		return spaces;
	}
}

public record Partition(bool Vertical, int Position);