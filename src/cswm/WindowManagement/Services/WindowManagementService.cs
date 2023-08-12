using System;
using cswm.WindowManagement.Arrangement;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement.Services;

public class WindowManagementService : IService
{
	private readonly ILogger _logger;
	private readonly IServiceProvider _provider;
	private readonly WinHookService _winHookService;
	private readonly WindowTrackingService _trackingService;
	private readonly WindowLayoutService _layoutService;

	public WindowManagementService(
		ILogger<WindowManagementService> logger,
		IServiceProvider provider,
		WinHookService winHookService,
		WindowTrackingService trackingService,
		WindowLayoutService layoutService
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(winHookService);
		ArgumentNullException.ThrowIfNull(trackingService);
		ArgumentNullException.ThrowIfNull(layoutService);

		_logger = logger;
		_provider = provider;
		_winHookService = winHookService;
		_trackingService = trackingService;
		_layoutService = layoutService;
	}

	public void Start()
	{
		_winHookService.Start();
		_trackingService.Start();
		_layoutService.Start();
		_layoutService.Rearrange();
	}

	public void Stop()
	{
		_layoutService.Stop();
		_trackingService.Stop();
		_winHookService.Stop();
	}

	public void SetArrangement<T>() where T : IArrangementStrategy
	{
		var arrangement = (T)_provider.GetService(typeof(T))!;
		_logger.LogDebug("Applying new arrangement {ArrangementType}", arrangement.GetType().Name);
		_layoutService.ArrangementStrategy = arrangement;
		_layoutService.Rearrange();
	}
}
