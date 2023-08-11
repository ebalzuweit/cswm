using System;
using cswm.WindowManagement.Arrangement.Layout;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;
public class WindowManagementService : IService
{
	private readonly ILogger _logger;
	private readonly WindowLayoutService _layoutService;

	public WindowManagementService(
		ILogger<WindowManagementService> logger,
		WindowLayoutService layoutService
	)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(layoutService);

		_logger = logger;
		_layoutService = layoutService;
	}

	public void Start()
	{
		_layoutService.Start();
	}

	public void Stop()
	{
		_layoutService.Stop();
	}

	public void SetLayoutMode<T>() where T : ILayoutMode, new()
	{
		_logger.LogInformation($"Creating new layout '{nameof(T)}'");
		var layout = new T();
		_layoutService.SetLayoutMode(layout);
	}
}
