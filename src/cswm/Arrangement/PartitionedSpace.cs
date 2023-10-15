using System;
using System.Collections.Generic;
using System.Linq;
using cswm.Services;
using cswm.WinApi;

namespace cswm.Arrangement;

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

	// FIXME: This is swapping windows consistently for certain resizes (e.g. Top resize?)
	public void ResizeSpace(Rect from, Rect to)
	{
		const int MaxWindowsPadding = 16;
		if (_partitions.Count == 0)
			return;

		// Horizontal resize
		if (from.Left != to.Left)
		{
			var partition = _partitions.Where(p => p.Vertical && p.Position <= from.Left + MaxWindowsPadding).FirstOrDefault();
			ResizePartition(partition, true, to.Left);
		}
		else if (from.Right != to.Right)
		{
			var partition = _partitions.Where(p => p.Vertical && p.Position >= from.Right - MaxWindowsPadding).FirstOrDefault();
			ResizePartition(partition, true, to.Right);
		}

		// Vertical resize
		if (from.Top != to.Top)
		{
			var partition = _partitions.Where(p => p.Vertical == false && p.Position <= from.Top).FirstOrDefault();
			ResizePartition(partition, false, to.Top);
		}
		else if (from.Bottom != to.Bottom)
		{
			var partition = _partitions.Where(p => p.Vertical == false && p.Position >= from.Bottom - MaxWindowsPadding).FirstOrDefault();
			ResizePartition(partition, false, to.Bottom);
		}

		UpdateSpacesCache();

		void ResizePartition(Partition? p, bool vertical, int position)
		{
			if (p is default(Partition))
				return;

			var idx = _partitions.IndexOf(p);
			_partitions[idx] = new(vertical, position);
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

		// TODO: determine verticalSplit by aspect ratio

		// Add partition
		_partitions.Add(new(verticalSplit, midpoint));
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