using cswm.Events;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace cswm.WindowManagement.Services;

/// <summary>
/// Performs window layout, based on an <see cref="IArrangementStrategy"/>.
/// </summary>
public sealed class WindowLayoutService : IService
{
    private readonly ILogger _logger;
    private readonly WindowManagementOptions _options;
    private readonly MessageBus _bus;
    private readonly WindowTrackingService _trackingService;

    private List<IDisposable> _subscriptions = new();
    private IEnumerable<MonitorLayout>? lastArrangement;

    public WindowLayoutService(
        ILogger<WindowLayoutService> logger,
        IOptions<WindowManagementOptions> options,
        MessageBus bus,
        WindowTrackingService trackingService,
        SplitArrangementStrategy defaultArrangementStrategy
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(defaultArrangementStrategy);

        _logger = logger;
        _options = options.Value;
        _bus = bus;
        _trackingService = trackingService;
        ArrangementStrategy = defaultArrangementStrategy;
    }

    public IArrangementStrategy ArrangementStrategy;

    public void Start()
    {
        Subscribe<SetArrangementStrategyEvent>(
            @event =>
            {
                ArrangementStrategy = @event.Strategy;
                Rearrange();
            });
        Subscribe<StartTrackingWindowEvent>(@event => OnWindowTrackingStart(@event.Window));
        Subscribe<StopTrackingWindowEvent>(@event => OnWindowTrackingStop(@event.Window));
        Subscribe<WindowMovedEvent>(@event => OnWindowMoved(@event.Window));
        Subscribe<OnTrackedWindowsResetEvent>(@event => OnWindowTrackingReset());

        Rearrange();

        void Subscribe<T>(Action<T> action)
            where T : Event
            => _subscriptions.Add(_bus.Subscribe<T>(action));
    }

    public void Stop()
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();
        _subscriptions.Clear();
    }

    private void Rearrange()
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