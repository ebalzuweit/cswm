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