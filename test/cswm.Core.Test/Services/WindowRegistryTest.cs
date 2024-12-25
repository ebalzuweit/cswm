using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Core.Test.Services;

public class WindowRegistryTest
{
	private readonly WindowRegistry windowRegistry;
	private Func<WindowInfo> GetWindowInfo = () => new WindowInfo(0, string.Empty, string.Empty, new(0, 0, 1, 1), false, false);

	public WindowRegistryTest(WindowRegistry windowRegistry)
	{
		this.windowRegistry = windowRegistry;
	}

	[Fact]
	public void RegisterWindow_ThrowsInvalidOperationException_IfWindowInfoNull()
	{
		var act = () => windowRegistry.RegisterWindow(null!);

		Assert.Throws<InvalidOperationException>(act);
	}

	[Fact]
	public void RegisterWindow_AddsWindowToRegistry_IfWindowInfoNotNull()
	{
		var window = GetWindowInfo();

		Assert.Null(windowRegistry.GetWindowInfo(window.Handle));

		windowRegistry.RegisterWindow(window);

		var result = windowRegistry.GetWindowInfo(window.Handle);
		Assert.NotNull(result);
		Assert.Equal(window, result);
	}

	[Fact]
	public void UnregisterWindow_ThrowsInvalidOperationException_IfWindowNull()
	{
		var act = () => { _ = windowRegistry.UnregisterWindow(null!); };

		Assert.Throws<InvalidOperationException>(act);
	}

	[Fact]
	public void UnregisterWindow_ReturnsFalse_IfWindowNotRegistered()
	{
		var window = GetWindowInfo();

		var result = windowRegistry.UnregisterWindow(window);

		Assert.False(result);
	}

	[Fact]
	public void UnregisterWindow_ReturnsTrue_IfWindowRegistered()
	{
		var window = GetWindowInfo();

		windowRegistry.RegisterWindow(window);
		var result = windowRegistry.UnregisterWindow(window);

		Assert.True(result);

		var info = windowRegistry.GetWindowInfo(window.Handle);
		Assert.Null(info);
	}

	[Fact]
	public void UpdateWindow_ThrowsInvalidOperationException_IfWindowNull()
	{
		var act = () => windowRegistry.UpdateWindow(null!);

		Assert.Throws<InvalidOperationException>(act);
	}

	[Fact]
	public void UpdateWindow_UpdatesWindowInfo_IfWindowRegistered()
	{
		var window = GetWindowInfo();

		windowRegistry.RegisterWindow(window);

		var registryInfo = windowRegistry.GetWindowInfo(window.Handle);
		Assert.NotNull(registryInfo);

		var updatedWindow = window with
		{
			Title = "New Title"
		};

		windowRegistry.UpdateWindow(updatedWindow);

		var updatedRegistryInfo = windowRegistry.GetWindowInfo(window.Handle);
		Assert.NotNull(updatedRegistryInfo);
		Assert.Equal(updatedWindow, updatedRegistryInfo);
	}

	[Fact]
	public void UpdateWindow_RegistersWindow_IfWindowNotRegistered()
	{
		var window = GetWindowInfo();

		windowRegistry.UpdateWindow(window);

		var registryInfo = windowRegistry.GetWindowInfo(window.Handle);
		Assert.NotNull(registryInfo);
		Assert.Equal(window, registryInfo);
	}
}
