using cswm.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cswm.Arrangement.Partitioning;

/// <summary>
/// Space partitioned with an internal <see cref="BspTree"/>.
/// </summary>
public sealed class BspSpace
{
    private readonly Rect _space;
    private BspTree _root = null!;
    private BspTree _maxDepthTree = null!;

    public BspSpace(Rect space)
    {
        _space = space;
    }

    public void SetTotalWindowCount(int spaceCount)
    {
        if (spaceCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(spaceCount));
        }

        // Rebuild partition tree
        var partitionCount = Math.Max(0, spaceCount - 1);
        _root = PartitionSpace(_space, partitionCount, prior: _maxDepthTree);
        if (_maxDepthTree == null)
        {
            // Initialize max depth tree
            _maxDepthTree = _root;
        }
        else if (_root.Count() >= _maxDepthTree.Count())
        {
            // Overwrite max depth tree
            _maxDepthTree = _root;
        }
        else
        {
            // Update max depth tree
            _maxDepthTree.CopyPartitions(_root);
        }
    }

    public IEnumerable<Rect> GetSpaces(int halfMargin = 0)
    {
        return AddWindowMargins_Recursive(_space, _root, halfMargin);

        IEnumerable<Rect> AddWindowMargins_Recursive(Rect space, BspTree node, int halfMargin)
        {
            if (node.IsLeaf)
            {
                yield return space;
                yield break;
            }

            // Add window margins
            (var left, var right) = space.SplitAt(node.Partition!.Vertical, node.Partition.Position);
            if (node.Partition.Vertical)
            {
                left = left.AddMargin(0, 0, halfMargin, 0); // left
                right = right.AddMargin(halfMargin, 0, 0, 0); // right
            }
            else
            {
                left = left.AddMargin(0, 0, 0, halfMargin); // top
                right = right.AddMargin(0, halfMargin, 0, 0); // bottom
            }

            // Traverse children
            foreach (var s in AddWindowMargins_Recursive(left, node.Left!, halfMargin))
                yield return s;
            foreach (var s in AddWindowMargins_Recursive(right, node.Right!, halfMargin))
                yield return s;
        }
    }

    public bool TryResize(Rect from, Rect to)
    {
        const int MaxWindowsPadding = 16;
        if (_root.Partition is null)
            return false;

        // Horizontal resize
        if (from.Left != to.Left)
        {
            var partition = GetLastPartitionWhere(true, p => p <= from.Left + MaxWindowsPadding);
            _ = ResizePartition(partition, to.Left);
        }
        else if (from.Right != to.Right)
        {
            var partition = GetLastPartitionWhere(true, p => p >= from.Right - MaxWindowsPadding);
            _ = ResizePartition(partition, to.Right);
        }

        // Vertical resize
        if (from.Top != to.Top)
        {
            var partition = GetLastPartitionWhere(false, p => p <= from.Top);
            _ = ResizePartition(partition, to.Top);
        }
        else if (from.Bottom != to.Bottom)
        {
            var partition = GetLastPartitionWhere(false, p => p >= from.Bottom - MaxWindowsPadding);
            _ = ResizePartition(partition, to.Bottom);
        }

        return true;

        BspTree? GetLastPartitionWhere(bool vertical, Func<int, bool> predicate)
        {
            BspTree? bsp = null;
            foreach (var p in _root)
            {
                if (p.Partition is null || p.Partition.Vertical != vertical)
                    continue;
                if (predicate(p.Partition.Position))
                {
                    bsp = p;
                }
            }
            return bsp;
        }

        bool ResizePartition(BspTree? p, int position)
        {
            if (p is null || p.Partition is null)
                return false;

            p.Partition.Position = position;
            return true;
        }
    }

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
    private BspTree PartitionSpace(Rect space, int partitionCount, int depth = 0, bool verticalSplit = true, BspTree? prior = null)
    {
        if (partitionCount < 1 || depth >= 3) // TODO: _options.MaxDepth
            return new BspTree();

        // Use prior partition if found
        var partition = prior?.Partition;
        if (partition is null)
        {
            // Determine partition position
            var start = verticalSplit ? space.Left : space.Top;
            var dimension = verticalSplit ? space.Width : space.Height;
            var midpoint = dimension / 2;
            if (dimension % 2 == 1)
                midpoint += 1; // left split gets the extra

            partition = new Partition(verticalSplit, start + midpoint);
        }

        // FIXME: determine verticalSplit by aspect ratio

        // Build sub-trees
        (var left, var right) = space.SplitAt(partition.Vertical, partition.Position);
        // TODO: _options.PreferRight to swap left and right partition
        partitionCount--; // subtract root partition
        var rightTree = PartitionSpace(right, partitionCount, depth + 1, !verticalSplit, prior?.Right);
        partitionCount -= rightTree.Where(x => x.Partition is not null).Count(); // subtract partitions created in right
        var leftTree = PartitionSpace(left, partitionCount, depth + 1, !verticalSplit, prior?.Left);

        return new BspTree(partition, leftTree, rightTree);
    }
}