using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace cswm.Arrangement;

/// <summary>
/// Binary-Space-Partitioning Tree.
/// </summary>
public sealed record BspTree() : IEnumerable<BspTree>
{
    public BspTree(Partition partition, BspTree left, BspTree right) : this()
    {
        Partition = partition;
        Left = left;
        Right = right;
    }

    public Partition? Partition { get; init; }
    public BspTree? Left { get; init; }
    public BspTree? Right { get; init; }

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
            sb.AppendLine($"{(IsLeaf ? "Leaf" : p.Partition!.ToString())}");
        }
        return sb.ToString();
    }
}

/// <summary>
/// Virtual separator between spaces.
/// </summary>
public sealed class Partition
{
    private readonly bool _vertical;

    public Partition(bool vertical, int position)
    {
        _vertical = vertical;
        Position = position;
    }

    /// <summary>
    /// <c>true</c> if the partition is vertical (separates left / right); otherwise, <c>false</c> and the partition is horizontal (separates top / bottom).
    /// </summary>
    public bool Vertical => _vertical;

    /// <summary>
    /// Position of the partition.
    /// </summary>
    public int Position { get; set; }

    public override string ToString()
    {
        return $"{(Vertical ? "|" : "—")} @ {Position}";
    }
}