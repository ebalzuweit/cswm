using System;

namespace cswm.Core.Layout;

public struct Bounds
{
	public readonly int Left { get; }
	public readonly int Right { get; }
	public readonly int Top { get; }
	public readonly int Bottom { get; }

	public readonly int Width => Right - Left;
	public readonly int Height => Bottom - Top;

	public Bounds(int left, int top, int right, int bottom)
	{
		if (left > right || top > bottom)
		{
			throw new InvalidOperationException($"Invalid dimensions for bounds: ({left}, {top}, {right}, {bottom})");
		}

		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}
}
