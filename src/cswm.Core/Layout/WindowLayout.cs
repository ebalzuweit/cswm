using System;

namespace cswm.Core.Layout;

public struct WindowLayout
{
	public readonly IntPtr Handle { get; }
	public readonly Bounds Area { get; }

	public WindowLayout(IntPtr handle, Bounds area)
	{
		Handle = handle;
		Area = area;
	}
}
