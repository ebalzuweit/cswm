using System;
using cswm.WindowManagement.Arrangement.Layout;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;
public class WindowManagementService : IService
{
	private readonly ILogger _logger;
	private readonly WinHookService _winHookService;
	private readonly WindowLayoutService _layoutService;

	public WindowManagementService(
		ILogger<WindowManagementService> logger,
		WinHookService winHookService,
		WindowLayoutService layoutService
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(winHookService);
		ArgumentNullException.ThrowIfNull(layoutService);

		_logger = logger;
		_winHookService = winHookService;
		_layoutService = layoutService;
	}

	public void Start()
	{
		_winHookService.Start();
		_layoutService.Start();
	}

	public void Stop()
	{
		_layoutService.Stop();
		_winHookService.Stop();
	}

	public void SetLayoutMode<T>() where T : ILayoutMode, new()
	{
		_logger.LogInformation($"Creating new layout '{typeof(T).Name}'");
		var layout = new T();
		_layoutService.SetLayoutMode(layout);
	}
}
