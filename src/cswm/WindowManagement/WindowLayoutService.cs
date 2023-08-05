using System;
using cswm.WindowManagement.Arrangement.Layout;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement;

public sealed class WindowLayoutService
{
	private readonly WindowManagementOptions _options;
	private readonly WindowTrackingService _windowTrackingService;

	private ILayoutMode _activeLayoutMode;

	public WindowLayoutService(
		IOptions<WindowManagementOptions> options,
		WindowTrackingService windowTrackingService
	)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(windowTrackingService);

		_options = options.Value;
		_windowTrackingService = windowTrackingService;
		_activeLayoutMode = new NoLayoutMode();
	}

	public void Start()
	{

	}

	public void Stop()
	{

	}
}