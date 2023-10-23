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
    public BspTree(Rect space)
    {
        Space = space;
    }

    public BspTree(Rect space, Partition partition, BspTree left, BspTree right)
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

    public bool IsLeaf => Partition is null;

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
            sb.AppendLine($"{p.Space}, {(IsLeaf ? "Leaf" : p.Partition!.ToString())}");
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