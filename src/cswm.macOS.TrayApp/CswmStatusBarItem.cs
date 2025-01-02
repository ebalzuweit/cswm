using System;
using System.Diagnostics;
using System.Reflection;
using AppKit;
using CoreGraphics;

namespace cswm.macOS.TrayApp;

public class CswmStatusBarItem
{
	private NSStatusItem? statusItem;

	public void AddToStatusBar()
	{
		statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
		statusItem.Button.AlternateTitle = "cswm";
		var icon = NSImage.ImageNamed("AppIcon");
		if (icon is not null)
		{
			icon.Size = new CGSize(18, 18);
			icon.Template = true;
			statusItem.Button.Image = icon;
		}

		statusItem.Menu = BuildMenu();
	}

	private NSMenu BuildMenu()
	{
		var statusMenu = new NSMenu();

		statusMenu.AddItem(BuildAboutMenuItem());
		statusMenu.AddItem(NSMenuItem.SeparatorItem);
		statusMenu.AddItem(BuildCloseMenuItem());

		return statusMenu;
	}

	private NSMenuItem BuildAboutMenuItem()
	{
		const string appName = "cswm";
		const string aboutUrl = "https://github.com/ebalzuweit/cswm";
		
		var assembly = Assembly.GetEntryAssembly()!;
		var version = assembly.GetName().Version!;
		var title = $"{appName} v{version.Major}.{version.Minor}.{version.Build}";
		
		return new NSMenuItem(title, OnClick);

		void OnClick(object? sender, EventArgs e)
			=> Process.Start(new ProcessStartInfo(aboutUrl) { UseShellExecute = true });
	}

	private NSMenuItem BuildCloseMenuItem()
	{
		return new NSMenuItem("Close", OnClick);
		
		void OnClick(object? sender, EventArgs e)
			=> NSApplication.SharedApplication.Terminate(statusItem);
	}
}