using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using cswm.Events;
using cswm.WindowManagement;

namespace cswm;

public class SystemTrayService
{
	private readonly MessageBus _bus;
	private NotifyIcon? _notifyIcon;

	public SystemTrayService(MessageBus bus)
	{
		_bus = bus ?? throw new ArgumentNullException(nameof(bus));
	}

	public void AddToSystemTray()
	{
		var thread = new Thread(() => SystemTrayApp())
		{
			Name = "cswmtray"
		};
		thread.Start();
	}

	public void RemoveFromSystemTray()
	{
		if (_notifyIcon is not null)
			_notifyIcon!.Visible = false;
		_notifyIcon?.Dispose();
	}

	private void SystemTrayApp()
	{
		var assembly = Assembly.GetEntryAssembly()!;
		var version = assembly.GetName().Version!;
		var menu = new ContextMenuStrip();
		menu.Items.Add(new ToolStripMenuItem() { Text = $"cswm {version.Major}.{version.Minor}.{version.Revision}", Enabled = false });
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add("Close", null, Close_OnClick);

		const string iconResourceName = "cswm.icon.ico";
		using (var stream = assembly.GetManifestResourceStream(iconResourceName))
		{
			_notifyIcon = new NotifyIcon
			{
				Icon = new Icon(stream!),
				ContextMenuStrip = menu,
				Text = "cswm",
				Visible = true,
			};
		}
		_notifyIcon.DoubleClick += NotifyIcon_OnDoubleClick;

		// message loop
		Application.Run();
	}

	private void NotifyIcon_OnDoubleClick(object? sender, EventArgs e)
		=> _bus.Publish(new ResetTrackedWindowsEvent());

	private void Close_OnClick(object? sender, EventArgs e)
		=> _bus.Publish(new ExitApplicationEvent());
}