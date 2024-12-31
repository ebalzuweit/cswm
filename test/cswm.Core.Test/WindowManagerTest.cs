using cswm.Core.Layout;
using cswm.Core.Models;
using cswm.Core.Services;
using cswm.Core.Test.Services;

namespace cswm.Core.Test;

public class WindowManagerTest
{
	private readonly IWindowController windowController;
	private readonly MockWindowEventHook windowEventHook;
	private readonly WindowManager windowManager;

	public WindowManagerTest(
		IWindowController windowController,
		MockWindowEventHook windowEventHook,
		WindowManager windowManager
	)
	{
		this.windowController = windowController;
		this.windowEventHook = windowEventHook;
		this.windowManager = windowManager;
	}

	[Fact]
	public void OnWindowCrated_AddWindowToLayout_IfWindowShouldBeManaged()
	{
		var monitor = windowController.GetMonitors().First();
		var window = new WindowInfo(0, string.Empty, string.Empty, new(0, 0, 1, 1), false, false);

		windowManager.Start();

		var windowsBefore = windowManager.GetWindows().ToArray();
		Assert.Empty(windowsBefore);

		windowEventHook.InvokeWindowCreated(window);

		var windowsAfter = windowManager.GetWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window.Handle &&
			x.Bounds == monitor.Bounds
		);
	}
}
