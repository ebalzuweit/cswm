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
		return CreateMonitorInfo(handle);
	}

	public MonitorInfo[] GetMonitors()
	{
		return [CreateMonitorInfo(0)];
	}

	public WindowInfo FetchWindowInfo(nint handle)
	{
		return windowRegistry.GetWindowInfo(handle) ?? CreateWindowInfo(handle);
	}

	public WindowInfo[] GetWindows()
	{
		return [.. windowRegistry.GetAllWindows()];
	}

	public void MoveWindow(IntPtr handle, Rect area)
	{
		var window = windowRegistry.GetWindowInfo(handle);
		if (window is null)
		{
			throw new InvalidOperationException($"Cannot move Window that is not registered. (Handle: {handle})");
		}
		windowRegistry.UpdateWindow(window with
		{
			Bounds = area
		});
	}

	private MonitorInfo CreateMonitorInfo(nint handle = 0, Rect? rect = null)
		=> new MonitorInfo(handle, rect ?? new(0, 0, 1920, 1080));

	private WindowInfo CreateWindowInfo(nint handle = 0, Rect? rect = null)
		=> new WindowInfo(handle, string.Empty, string.Empty, rect ?? new(0, 0, 1, 1), false, false);
}
