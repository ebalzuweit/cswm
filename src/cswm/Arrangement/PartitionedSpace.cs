using System;
using System.Collections.Generic;
using System.Linq;
using cswm.Services;
using cswm.WinApi;

namespace cswm.Arrangement;

// FIXME: Revisit naming of class & methods
public sealed class PartitionedSpace
{
	private readonly Rect _space;
	private readonly WindowManagementOptions _options;
	private List<Partition> _partitions = new();
	private List<Rect> _spaces = new List<Rect>();

	public PartitionedSpace(Rect space, WindowManagementOptions options)
	{
		_space = space;
		_options = options;
	}

	public void SetTotalWindowCount(int spacesCount)
	{
		var prevPartitions = new List<Partition>(_partitions);
		var space = _space.AddMargin(_options.MonitorPadding);

		// Create new partitions
		_partitions.Clear();
		_ = PartitionSpace(space, spacesCount).ToList();

		// Overwrite with previous partitions
		for (var i = 0; i < _partitions.Count() && i < prevPartitions.Count(); i++)
		{
			if (_partitions[i].Vertical == prevPartitions[i].Vertical)
				_partitions[i] = prevPartitions[i];
		}

		UpdateSpacesCache();
	}

	public void ResizeSpace(Rect from, Rect to)
	{
		const int MaxWindowsPadding = 16;
		if (_partitions.Count == 0)
			return;

		// Horizontal resize
		if (from.Left != to.Left)
		{
			var partition = GetLastPartitionWhere(true, p => p <= from.Left + MaxWindowsPadding);
			ResizePartition(partition, to.Left);
		}
		else if (from.Right != to.Right)
		{
			var partition = GetLastPartitionWhere(true, p => p >= from.Right - MaxWindowsPadding);
			ResizePartition(partition, to.Right);
		}

		// Vertical resize
		if (from.Top != to.Top)
		{
			var partition = GetLastPartitionWhere(false, p => p <= from.Top);
			ResizePartition(partition, to.Top);
		}
		else if (from.Bottom != to.Bottom)
		{
			var partition = GetLastPartitionWhere(false, p => p >= from.Bottom - MaxWindowsPadding);
			ResizePartition(partition, to.Bottom);
		}

		UpdateSpacesCache();

		Partition? GetLastPartitionWhere(bool vertical, Func<int, bool> predicate)
		{
			Partition? np = null;
			foreach (var p in _partitions)
			{
				if (p.Vertical != vertical)
					continue;
				if (predicate(p.Position))
				{
					np = p;
				}
			}
			return np;
		}

		void ResizePartition(Partition? p, int position)
		{
			if (p is default(Partition))
				return;

			var idx = _partitions.IndexOf(p);
			_partitions[idx] = new(p.Vertical, position);
		}
	}

	public IList<Rect> GetWindowSpaces()
	{
		return _spaces;
	}

	private void UpdateSpacesCache()
	{
		_spaces = BuildSpacesFromPartitions();
	}

	/// <summary>
	/// Partition a space recursively.
	/// </summary>
	/// <remarks>
	/// This functions like depth-first search with a max-depth.
	/// The right half of the tree is given preference, by default.
	/// </remarks>
	/// <param name="space">Space to partition.</param>
	/// <param name="sectionCount">Number of sections to partition the space into.</param>
	/// <param name="depth">Current depth of the tree traversal.</param>
	/// <param name="verticalSplit">If the partition should be made with a vertical split.</param>
	/// <returns>Partitions of the given space.</returns>
	private IEnumerable<Rect> PartitionSpace(Rect space, int sectionCount, int depth = 0, bool verticalSplit = true)
	{
		if (sectionCount <= 0)
			return Array.Empty<Rect>();
		if (sectionCount == 1 || depth >= 3) // TODO: _options.MaxDepth
			return new[] { space };

		// Determine partition position
		var dimension = verticalSplit ? space.Width : space.Height;
		var midpoint = dimension / 2;
		if (dimension % 2 == 1)
			midpoint += 1; // left split gets the extra

		// FIXME: determine verticalSplit by aspect ratio

		// Add partition
		var start = verticalSplit ? space.Left : space.Top;
		_partitions.Add(new(verticalSplit, start + midpoint));
		(var left, var right) = space.SplitAt(verticalSplit, midpoint);

		// Add window margins
		var halfMargin = _options.WindowMargin / 2;
		if (verticalSplit)
		{
			left = left.AddMargin(0, 0, halfMargin, 0); // left
			right = right.AddMargin(halfMargin, 0, 0, 0); // right
		}
		else
		{
			left = left.AddMargin(0, 0, 0, halfMargin); // top
			right = right.AddMargin(0, halfMargin, 0, 0); // bottom
		}

		// TODO: _options.PreferRight to swap left and right partition

		// Recurse into each split
		var rightSections = PartitionSpace(right, sectionCount - 1, depth + 1, !verticalSplit);
		var leftSections = PartitionSpace(left, sectionCount - rightSections.Count(), depth + 1, !verticalSplit);

		// Join results
		var sections = leftSections.Concat(rightSections);
		return sections;
	}

	private List<Rect> BuildSpacesFromPartitions()
	{
		var s = _space.AddMargin(_options.MonitorPadding);
		if (_partitions.Count == 0)
		{
			// Shortcut if there's no partitions
			return new List<Rect>() { s };
		}

		var halfMargin = _options.WindowMargin / 2;
		var spaces = new List<Rect>();
		foreach (var p in _partitions)
		{
			var start = p.Vertical ? s.Left : s.Top;
			(var l, var r) = s.SplitAt(p.Vertical, p.Position - start);
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