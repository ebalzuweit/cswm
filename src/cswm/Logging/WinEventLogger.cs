using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using cswm.Events;
using cswm.WinApi;
using Microsoft.Extensions.Logging;

namespace cswm.Logging;

public class WinEventLogger
{
	private readonly ILogger _logger;
	private readonly MessageBus _bus;

	private ICollection<Func<WindowEvent, bool>> _predicates = new List<Func<WindowEvent, bool>>();
	private ICollection<Func<WindowEvent, string>> _formats = new List<Func<WindowEvent, string>>();

	public WinEventLogger(ILogger<WinEventLogger> logger, MessageBus bus)
	{
		_logger = logger;
		_bus = bus ?? throw new ArgumentNullException(nameof(bus));

		_bus.Events.Where(@event => @event is WindowEvent)
			.Subscribe(@event => On_WindowEvent((WindowEvent)@event));
	}

	public void AddFilter(Func<WindowEvent, bool> predicate)
	{
		_predicates.Add(predicate);
	}

	public void AddFormat(Func<WindowEvent, string> format)
	{
		_formats.Add(format);
	}

	private void On_WindowEvent(WindowEvent @event)
	{
		if (_predicates.Any(predicate => predicate(@event) == false)) return;
		foreach (var format in _formats)
			_logger.LogDebug(format(@event));
	}
}