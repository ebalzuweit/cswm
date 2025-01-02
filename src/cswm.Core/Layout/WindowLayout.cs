using System;

namespace cswm.Core.Layout;

public struct WindowLayout
{
	public readonly IntPtr Handle { get; }
	public readonly Rect Area { get; }

	public WindowLayout(IntPtr handle, Rect area)
	{
		Handle = handle;
		Area = area;
	}
}
