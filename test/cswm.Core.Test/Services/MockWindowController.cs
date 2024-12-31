using cswm.Core.Layout;
using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Core.Test.Services;

public class MockWindowController : IWindowController
{
	private readonly WindowRegistry windowRegistry;

	public MockWindowController(WindowRegistry windowRegistry)
	{
		this.windowRegistry = windowRegistry;
	}

	public MonitorInfo FetchMonitorInfo(nint handle)
	{
		return GetMonitorInfo(handle);
	}

	public MonitorInfo[] GetMonitors()
	{
		return [GetMonitorInfo(0)];
	}

	public WindowInfo FetchWindowInfo(nint handle)
	{
		return GetWindowInfo(handle);
	}

	public WindowInfo[] GetWindows()
	{
		return [.. windowRegistry.GetAllWindows()];
	}

	public void MoveWindow(IntPtr handle, Rect area) { }

	private MonitorInfo GetMonitorInfo(nint handle = 0, Rect? rect = null)
		=> new MonitorInfo(handle, rect ?? new(0, 0, 1920, 1080));

	private WindowInfo GetWindowInfo(nint handle = 0, Rect? rect = null)
		=> new WindowInfo(handle, string.Empty, string.Empty, rect ?? new(0, 0, 1, 1), false, false);
}
