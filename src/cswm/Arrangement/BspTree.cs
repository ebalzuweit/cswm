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

    public Rect Space { get; set; }
    public Partition? Partition { get; set; }
    public BspTree? Left { get; set; }
    public BspTree? Right { get; set; }

    public (Rect, Rect) CalcSplits()
    {
        if (Partition is null)
            throw new NullReferenceException("Cannot calculate splits for null partition.");

        (var left, var right) = Space.SplitAt(Partition.Vertical, Partition.Position);
        return (left, right);
    }

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
            sb.AppendLine($"{p.Space}, {p?.Partition?.ToString() ?? "🚫"}");
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
        return $"{(Vertical ? "|" : "—")} @ {Position}";
    }
}