using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement;

public sealed class WindowLayoutService : IService
{
	private readonly ILogger _logger;
	private readonly WindowManagementOptions _options;
	private readonly WindowTrackingService _trackingService;

	public WindowLayoutService(
		ILogger<WindowLayoutService> logger,
		IOptions<WindowManagementOptions> options,
		WindowTrackingService trackingService,
		SplitArrangementStrategy defaultArrangementStrategy
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(trackingService);
		ArgumentNullException.ThrowIfNull(defaultArrangementStrategy);

		_logger = logger;
		_options = options.Value;
		_trackingService = trackingService;
		ArrangementStrategy = defaultArrangementStrategy;
	}

	public IArrangementStrategy ArrangementStrategy;

	public void Start()
	{
		_trackingService.OnTrackedWindowsReset += OnWindowTrackingReset;
		_trackingService.OnWindowTrackingStart += OnWindowTrackingStart;
		_trackingService.OnWindowtrackingStop += OnWindowTrackingStop;
		_trackingService.OnWindowMoved += OnWindowMoved;
	}

	public void Stop()
	{
#pragma warning disable CS8601
		_trackingService.OnTrackedWindowsReset -= OnWindowTrackingReset;
		_trackingService.OnWindowTrackingStart -= OnWindowTrackingStart;
		_trackingService.OnWindowtrackingStop -= OnWindowTrackingStop;
		_trackingService.OnWindowMoved -= OnWindowMoved;
#pragma warning restore CS8601
	}

	public void Rearrange()
	{
		UpdateWindowPositions();
	}

	private void OnWindowTrackingReset()
	{
		Rearrange();
	}

	private void OnWindowTrackingStart(Window window)
	{
		UpdateWindowPositions();
	}

	private void OnWindowTrackingStop(Window window)
	{
		UpdateWindowPositions();
	}

	private void OnWindowMoved(Window window)
	{
		UpdateWindowPositions();
	}

	private void UpdateWindowPositions(Window? preferredWindow = default)
	{
		var monitors = User32.EnumDisplayMonitors()
			.Select(hMonitor => new Monitor(hMonitor))
			.ToArray();
		var windows = _trackingService.Windows;
		var monitorLayouts = monitors.Select(monitor =>
			new MonitorLayout(
				monitor.hMonitor,
				monitor.WorkArea,
				windows.Where(w => User32.MonitorFromWindow(w.hWnd, MonitorFlags.DefaultToNearest) == monitor.hMonitor)
					.Select(w => new WindowLayout(w, w.Position))
			)
		);
		var windowLayouts = preferredWindow is null
			? ArrangementStrategy.Arrange(monitorLayouts)
			: ArrangementStrategy.ArrangeOnWindowMove(monitorLayouts, preferredWindow);
		foreach (var layout in windowLayouts)
			SetWindowPos(layout.Window, layout.Position);
	}

	private bool SetWindowPos(Window window, Rect position)
	{
		var windowsPadding = (window.Position.Width - window.ClientPosition.Width) / 2;
		var adjustedPosition = new Rect(
			left: position.Left - windowsPadding,
			top: position.Top,
			right: position.Right + windowsPadding,
			bottom: position.Bottom + windowsPadding);

		_logger.LogDebug("Moving {Window} to {Position}", window, position);
		if (_options.DoNotManage)
			return true;
		return User32.SetWindowPos(
			window.hWnd,
			HwndInsertAfterFlags.HWND_NOTOPMOST,
			x: adjustedPosition.Left,
			y: adjustedPosition.Top,
			cx: adjustedPosition.Width,
			cy: adjustedPosition.Height,
			SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
	}
}