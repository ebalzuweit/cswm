using System;
using System.Collections.Generic;
using System.Linq;
using cswm.Events;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement.Services;

public sealed class WindowLayoutService : IService
{
	private readonly ILogger _logger;
	private readonly WindowManagementOptions _options;
	private readonly IServiceProvider _provider;
	private readonly MessageBus _bus;
	private readonly WindowTrackingService _trackingService;

	private List<IDisposable> _subscriptions = new();
	private IEnumerable<MonitorLayout>? lastArrangement;

	public WindowLayoutService(
		ILogger<WindowLayoutService> logger,
		IOptions<WindowManagementOptions> options,
		IServiceProvider provider,
		MessageBus bus,
		WindowTrackingService trackingService,
		SplitArrangementStrategy defaultArrangementStrategy
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(bus);
		ArgumentNullException.ThrowIfNull(trackingService);
		ArgumentNullException.ThrowIfNull(defaultArrangementStrategy);

		_logger = logger;
		_options = options.Value;
		_provider = provider;
		_bus = bus;
		_trackingService = trackingService;
		ArrangementStrategy = defaultArrangementStrategy;
	}

	public IArrangementStrategy ArrangementStrategy;
	public IEnumerable<MonitorLayout> LastArrangement => lastArrangement ?? Array.Empty<MonitorLayout>();

	public void Start()
	{
		_subscriptions.Add(_bus.Subscribe<SetArrangementStrategyEvent>(
			@event =>
			{
				ArrangementStrategy = @event.Strategy;
				Rearrange();
			})
		);
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
		UpdateWindowPositions(window);
	}

	private void UpdateWindowPositions(Window? movedWindow = default)
	{
		var monitorLayouts = _trackingService.GetCurrentLayouts();
		lastArrangement = movedWindow is null
			? ArrangementStrategy.Arrange(monitorLayouts)
			: ArrangementStrategy.ArrangeOnWindowMove(monitorLayouts, movedWindow);
		foreach (var layout in lastArrangement)
		{
			foreach (var windowLayout in layout.Windows)
				SetWindowPos(windowLayout.Window, windowLayout.Position);
		}
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