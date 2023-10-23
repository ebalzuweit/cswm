namespace cswm.Arrangement.Partitioning;

/// <summary>
/// Virtual separator between spaces.
/// </summary>
public sealed class Partition
{
	private readonly bool _vertical;
	private int _position;

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
	public int Position
	{
		get => _position;
		set
		{
			_position = value > 0 ? value : 0;
		}
	}

	public override string ToString()
	{
		return $"{(Vertical ? "|" : "—")} @ {Position}";
	}
}