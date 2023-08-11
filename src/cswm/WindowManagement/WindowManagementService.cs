using System;
using cswm.WindowManagement.Arrangement.Layout;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;
public class WindowManagementService : IService
{
	private readonly ILogger _logger;
	private readonly WinHookService _winHookService;
	private readonly WindowTrackingService _trackingService;
	private readonly WindowLayoutService _layoutService;

	public WindowManagementService(
		ILogger<WindowManagementService> logger,
		WinHookService winHookService,
		WindowTrackingService trackingService,
		WindowLayoutService layoutService
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(winHookService);
		ArgumentNullException.ThrowIfNull(trackingService);
		ArgumentNullException.ThrowIfNull(layoutService);

		_logger = logger;
		_winHookService = winHookService;
		_trackingService = trackingService;
		_layoutService = layoutService;
	}

	public void Start()
	{
		_winHookService.Start();
		_trackingService.Start();
		_layoutService.Start();
	}

	public void Stop()
	{
		_layoutService.Stop();
		_trackingService.Stop();
		_winHookService.Stop();
	}

	public void SetLayoutMode<T>() where T : ILayoutMode, new()
	{
		_logger.LogInformation($"Creating new layout '{typeof(T).Name}'");
		var layout = new T();
		_layoutService.SetLayoutMode(layout);
	}
}
