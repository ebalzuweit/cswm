using cswm.WinApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace cswm.Arrangement;

/// <summary>
/// Binary-Space-Partitioning Tree.
/// </summary>
public sealed class BspTree : IEnumerable<BspTree>
{
    public BspTree(Rect space, Partition? partition = null, BspTree? left = null, BspTree? right = null)
    {
        Space = space;
        Partition = partition;
        Left = left;
        Right = right;
    }

    public Rect Space { get; private set; }
    public Partition? Partition { get; private set; }
    public BspTree? Left { get; set; }
    public BspTree? Right { get; set; }

    public bool TryResize(Rect from, Rect to)
    {
        const int MaxWindowsPadding = 16;
        if (Partition is null)
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
            foreach (var p in this)
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

            p.Partition = new(p.Partition.Vertical, position);
            return true;
        }
    }

    public (Rect, Rect) CalcSplits()
    {
        if (Partition is null)
            throw new NullReferenceException("Cannot calculate splits for null partition.");

        (var left, var right) = Space.SplitAt(Partition.Vertical, Partition.Position);
        return (left, right);
    }

    // FIXME: Space calculations are going wrong at 4+ partitions
    public IEnumerable<Rect> CalcSpaces(int halfMargin = 0)
    {
        if (Partition is null)
        {
            yield return Space;
            yield break;
        }

        (var left, var right) = CalcSplits();

        // Add window margins
        if (Partition.Vertical)
        {
            left = left.AddMargin(0, 0, halfMargin, 0); // left
            right = right.AddMargin(halfMargin, 0, 0, 0); // right
        }
        else
        {
            left = left.AddMargin(0, 0, 0, halfMargin); // top
            right = right.AddMargin(0, halfMargin, 0, 0); // bottom
        }

        if (Left is not null)
        {
            Left.Space = left;
            foreach (var space in Left.CalcSpaces(halfMargin))
                yield return space;
        }
        if (Right is not null)
        {
            Right.Space = right;
            foreach (var space in Right.CalcSpaces(halfMargin))
                yield return space;
        }
    }

    public IEnumerator<BspTree> GetEnumerator()
    {
        yield return this;
        if (Left is not null)
        {
            foreach (var p in Left)
                yield return p;
        }
        if (Right is not null)
        {
            foreach (var p in Right)
                yield return p;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var p in this)
        {
            sb.AppendLine($"{p.Space}, {p?.Partition}");
        }
        return sb.ToString();
    }
}

/// <summary>
/// Virtual separator between spaces.
/// </summary>
/// <param name="Vertical"><c>true</c> if the partition is vertical (separates left / right); otherwise, <c>false</c> and the partition is horizontal (separates top / bottom).</param>
/// <param name="Position">Position of the partition.</param>
public sealed record Partition(bool Vertical, int Position)
{
    public override string ToString()
    {
        return $"{(Vertical ? "|" : "——")} @ {Position}";
    }
}