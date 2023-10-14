using System;
using System.Collections.Generic;
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

	public void SetTotalSpacesCount(int spacesCount)
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
	public IList<Rect> GetSpaces()
	{
		if (_partitions.Count == 0)
		{
			return new[] { _space };
		}

		var spaces = new List<Rect>();
		var s = _space;
		foreach (var p in _partitions)
		{
			(var l, var r) = s.SplitAt(p.Vertical, p.Position);
			spaces.Add(l);

			s = r;
		}
		spaces.Add(s);
		return spaces;
	}
}

public record Partition(bool Vertical, int Position);