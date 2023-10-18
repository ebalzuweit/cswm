using cswm.Services;
using cswm.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cswm.Arrangement;

/// <summary>
/// Space arranged with an internal <see cref="BspTree"/>.
/// </summary>
public sealed class BspSpace
{
    private readonly Rect _space;
    private readonly WindowManagementOptions _options;
    private BspTree _root = null!;

    public BspSpace(Rect space, WindowManagementOptions options)
    {
        _space = space;
        _options = options;
    }

    // FIXME: Keep previous positions when partitions added / removed
    public void SetTotalWindowCount(int spacesCount)
    {
        var space = _space.AddMargin(_options.MonitorPadding);
        var partitionCount = Math.Max(0, spacesCount - 1);

        // Rebuild partition tree
        _root = PartitionSpace(space, partitionCount);
    }

    public IEnumerable<Rect> GetSpaces(int halfMargin) => _root.CalcSpaces(halfMargin);

    public bool TryResize(Rect from, Rect to) => _root.TryResize(from, to);

    /// <summary>
    /// Partition a space recursively.
    /// </summary>
    /// <remarks>
    /// This builds the tree by depth-first search traversal with a max-depth.
	/// The right half of the tree is preferred for partitioning, giving larger spaces to the "left" spaces.
    /// </remarks>
    /// <param name="space">Space to partition.</param>
    /// <param name="partitionCount">Number of partitions to create.</param>
    /// <param name="depth">Current depth of tree traversal.</param>
    /// <param name="verticalSplit">If the partition should have a vertical split.</param>
    /// <returns><see cref="BspTree"/> partitioning the space.</returns>
    private BspTree PartitionSpace(Rect space, int partitionCount, int depth = 0, bool verticalSplit = true)
    {
        if (partitionCount < 1 || depth >= 3) // TODO: _options.MaxDepth
            return new(space);

        // Determine partition position
        var dimension = verticalSplit ? space.Width : space.Height;
        var midpoint = dimension / 2;
        if (dimension % 2 == 1)
            midpoint += 1; // left split gets the extra

        // FIXME: determine verticalSplit by aspect ratio

        // Add partition
        var start = verticalSplit ? space.Left : space.Top;
        var partition = new Partition(verticalSplit, start + midpoint);
        var root = new BspTree(space, partition);

        (var left, var right) = root.CalcSplits();

        // TODO: _options.PreferRight to swap left and right partition

        root.Right = PartitionSpace(right, partitionCount - 1 /* root partition */, depth + 1, !verticalSplit);
        root.Left = PartitionSpace(left, partitionCount - 1 - root.Right.Count(), depth + 1, !verticalSplit);

        return root;
    }
}