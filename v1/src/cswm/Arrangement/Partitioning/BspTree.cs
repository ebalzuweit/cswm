using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace cswm.Arrangement.Partitioning;

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

    public Partition? Partition { get; internal set; }
    public BspTree? Left { get; init; }
    public BspTree? Right { get; init; }

    public bool IsLeaf => Partition is null;

    /// <summary>
    /// Recursively copy partitions from <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    public void CopyPartitions(BspTree other)
    {
        if (IsLeaf == false && other.IsLeaf == false)
        {
            Partition = other.Partition;
        }
        if (Left != null && other.Left != null &&
            Left.IsLeaf == false && other.Left.IsLeaf == false)
        {
            Left.CopyPartitions(other.Left);
        }
        if (Right != null && other.Right != null &&
            Right.IsLeaf == false && other.Right.IsLeaf == false)
        {
            Right.CopyPartitions(other.Right);
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
            sb.AppendLine($"{(IsLeaf ? "Leaf" : p.Partition!.ToString())}");
        }
        return sb.ToString();
    }
}