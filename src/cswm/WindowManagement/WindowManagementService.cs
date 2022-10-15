using System;
using System.Linq;
using System.Text;
using cswm.Events;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;

public class WindowManagementService
{
	private readonly ILogger? _logger;
	private readonly WindowManagementOptions _options;
	private readonly WindowTrackingService _windowTrackingService;
	private readonly IArrangementStrategy _arrangementStrategy;

	public WindowManagementService(
		ILogger<WindowManagementService> logger,
		IConfiguration configuration,
		MessageBus bus,
		WindowTrackingService windowTrackingService,
		IArrangementStrategy arrangementStrategy)
	{
		_logger = logger;
		_options = configuration.GetSection(nameof(WindowManagementOptions)).Get<WindowManagementOptions>() ?? new WindowManagementOptions();
		_windowTrackingService = windowTrackingService ?? throw new ArgumentNullException(nameof(windowTrackingService));
		_arrangementStrategy = arrangementStrategy ?? throw new ArgumentNullException(nameof(arrangementStrategy));
	}

	public void Start()
	{
		_windowTrackingService.OnTrackedWindowsReset += OnWindowTrackingReset;
		_windowTrackingService.OnWindowTrackingStart += OnWindowTrackingStart;
		_windowTrackingService.OnWindowtrackingStop += OnWindowTrackingStop;
		_windowTrackingService.OnWindowMoved += OnWindowMoved;
	}

	public void Stop()
	{
#pragma warning disable CS8601
		_windowTrackingService.OnTrackedWindowsReset -= OnWindowTrackingReset;
		_windowTrackingService.OnWindowTrackingStart -= OnWindowTrackingStart;
		_windowTrackingService.OnWindowtrackingStop -= OnWindowTrackingStop;
		_windowTrackingService.OnWindowMoved -= OnWindowMoved;
#pragma warning restore CS8601
	}

	private void UpdateWindowPositions()
	{
		_logger?.LogDebug("Updating window positions");
		var monitors = WinApi.User32.EnumDisplayMonitors()
			.Select(hMonitor => new Monitor(hMonitor))
			.ToArray();
		var windows = _windowTrackingService.Windows;
		var monitorLayouts = monitors.Select(monitor =>
			new MonitorLayout(
				monitor.hMonitor,
				monitor.WorkArea,
				windows.Where(w => User32.MonitorFromWindow(w.hWnd, MonitorFlags.DefaultToNearest) == monitor.hMonitor)
					.Select(w => new WindowLayout(w.hWnd, w.Position))
			)
		);
		var windowLayouts = _arrangementStrategy.Arrange(monitorLayouts);
		foreach (var layout in windowLayouts)
			SetWindowPos(new Window((Windows.Win32.Foundation.HWND)layout.hWnd), layout.Position); // TODO: don't recreate Window object
	}

	private bool SetWindowPos(Window window, Rect position)
	{
		var windowsPadding = (window.Position.Width - window.ClientPosition.Width) / 2;
		var adjustedPosition = new Rect(
			left: position.Left - windowsPadding,
			top: position.Top,
			right: position.Right + windowsPadding,
			bottom: position.Bottom + windowsPadding);

		return User32.SetWindowPos(
			window.hWnd,
			HwndInsertAfterFlags.HWND_NOTOPMOST,
			x: adjustedPosition.Left + _options.Padding,
			y: adjustedPosition.Top + _options.Padding,
			cx: adjustedPosition.Width - (_options.Padding * 2),
			cy: adjustedPosition.Height - (_options.Padding * 2),
			SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
	}

	private void OnWindowTrackingReset()
	{
		var windows = _windowTrackingService.Windows;
		var sb = new StringBuilder("Tracked windows:\n");
		foreach (var window in windows)
		{
			sb.AppendLine("\t" + window.ToString());
		}
		_logger?.LogInformation(sb.ToString());

		UpdateWindowPositions();
	}

	private void OnWindowTrackingStart(Window window)
	{
		_logger?.LogInformation("Started tracking window: {window}", window);
		UpdateWindowPositions();
	}

	private void OnWindowTrackingStop(Window window)
	{
		_logger?.LogInformation("Stopped tracking window: {window}", window);
		UpdateWindowPositions();
	}

	private void OnWindowMoved(Window window)
	{
		_logger?.LogInformation("Window moved: {window}", window);
		UpdateWindowPositions();
	}
}