using System;
using System.Collections.Generic;
using System.Text;
using cswm.WindowManagement.Arrangement.Layout;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement;

public sealed class WindowLayoutService : IService
{
	private readonly ILogger _logger;
	private readonly WindowManagementOptions _options;
	private readonly WindowTrackingService _trackingService;

	private ILayoutMode _activeLayoutMode;
	private ICollection<Window> _activeWindows = new List<Window>();

	public WindowLayoutService(
		ILogger<WindowLayoutService> logger,
		IOptions<WindowManagementOptions> options,
		WindowTrackingService trackingService
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(trackingService);

		_logger = logger;
		_options = options.Value;
		_trackingService = trackingService;
	}

	public string ActiveLayoutDisplayName => _activeLayoutMode.DisplayName;
	public Type ActiveLayoutMode => _activeLayoutMode.GetType();

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

	public void SetLayoutMode(ILayoutMode layoutMode)
	{
		_activeLayoutMode = layoutMode;
		_activeLayoutMode.Initialize(_activeWindows);
	}

	public void RelayoutWindows()
	{
		_logger.LogDebug("Laying out windows from scratch...");
		LogTrackedWindows();
	}

	/*
	* TODO: We're tracking all windows, but we only want to layout visible, non-minimized windows
	*/

	private void OnWindowTrackingReset()
	{
		RelayoutWindows();
	}

	private void OnWindowTrackingStart(Window window)
	{

	}

	private void OnWindowTrackingStop(Window window)
	{

	}

	private void OnWindowMoved(Window window)
	{

	}

	private void LogTrackedWindows()
	{
		var windows = _trackingService.VisibleWindows;
		var sb = new StringBuilder();
		sb.Append($"{windows.Count} visible windows:");
		foreach (var window in windows)
		{
			sb.Append($"\n - {window.Caption} [{window.ClassName}]");
		}
		_logger.LogDebug(sb.ToString());
	}
}