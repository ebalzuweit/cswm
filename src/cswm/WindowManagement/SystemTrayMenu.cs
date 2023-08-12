using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using cswm.Events;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Services;
using Humanizer;

namespace cswm.WindowManagement;

public class SystemTrayMenu
{
	private readonly MessageBus _bus;
	private readonly WindowLayoutService _layoutService;

	public SystemTrayMenu(MessageBus bus, WindowLayoutService layoutService)
	{
		ArgumentNullException.ThrowIfNull(bus);
		ArgumentNullException.ThrowIfNull(layoutService);

		_bus = bus;
		_layoutService = layoutService;
	}

	public ToolStripItem[] BuildTrayMenu()
	{
		var items = new List<ToolStripItem>
		{
			BuildAboutMenuItem(),
			new ToolStripSeparator()
		};
		if (_layoutService.LastArrangement is not null)
		{
			items.AddRange(BuildMonitorMenuItems(_layoutService.LastArrangement));
		}
		items.Add(new ToolStripSeparator());
		items.Add(BuildCloseMenuItem());

		return items.ToArray();
	}

	private ToolStripMenuItem BuildAboutMenuItem()
	{
		var assembly = Assembly.GetEntryAssembly()!;
		var version = assembly.GetName().Version!;
		var caption = $"cswm v{version.Major}.{version.Minor}.{version.Build}";
		return new(caption, null, OnClick);

		void OnClick(object? sender, EventArgs eventArgs)
			=> Process.Start(new ProcessStartInfo("https://github.com/ebalzuweit/cswm") { UseShellExecute = true });
	}

	private ToolStripMenuItem BuildCloseMenuItem()
	{
		return new("Close", null, OnClick);

		void OnClick(object? sender, EventArgs eventArgs)
			=> _bus.Publish(new ExitApplicationEvent());
	}

	private ToolStripMenuItem[] BuildMonitorMenuItems(IEnumerable<MonitorLayout> layouts)
	{
		return layouts.Select(BuildMonitorMenuItem).ToArray();

		ToolStripMenuItem BuildMonitorMenuItem(MonitorLayout layout)
<<<<<<< HEAD
			=> new(layout.Monitor.DeviceName, null, layout.Windows.Select(BuildWindowMenuItem).ToArray());

=======
			=> new("Monitor", null, layout.Windows.Select(BuildWindowMenuItem).ToArray());
>>>>>>> 91d0cb2 (Move tray menu logic to SystemTrayMenu)
		ToolStripMenuItem BuildWindowMenuItem(WindowLayout windowLayout)
			=> new(windowLayout.Window.Caption.Truncate(40));
	}
}