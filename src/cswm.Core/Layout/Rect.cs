using System;
using System.Runtime.InteropServices;

namespace cswm.Core.Layout;

[StructLayout(LayoutKind.Sequential)]
public struct Rect
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
}
