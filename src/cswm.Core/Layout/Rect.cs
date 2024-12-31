using System;
using System.Runtime.InteropServices;

namespace cswm.Core.Layout;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect
{
	public readonly int Left { get; }
	public readonly int Right { get; }
	public readonly int Top { get; }
	public readonly int Bottom { get; }

	public readonly int Width => Right - Left;
	public readonly int Height => Bottom - Top;

	public Rect(int left, int top, int right, int bottom)
	{
		if (left > right || top > bottom)
		{
			throw new InvalidOperationException($"Invalid dimensions for {typeof(Rect)}: ({left}, {top}, {right}, {bottom})");
		}

		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public override string ToString()
	{
		return $"({Left}, {Top}, {Right}, {Bottom})";
	}

	public override bool Equals(object? o)
	{
		if (o is null)
		{
			return false;
		}
		if (o is Rect r)
		{
			return Left == r.Left &&
				Top == r.Top &&
				Right == r.Right &&
				Bottom == r.Bottom;
		}

		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(Rect a, Rect b) => a.Equals(b);

	public static bool operator !=(Rect a, Rect b) => a.Equals(b) == false;
}
