using System;
using cswm.Events;
using cswm.WindowManagement.Arrangement;

namespace cswm.WindowManagement;

public class SetArrangementStrategyEvent : Event
{
	private readonly IArrangementStrategy _strategy;
	private readonly Monitor? _monitor;

	public SetArrangementStrategyEvent(IArrangementStrategy strategy, Monitor? monitor = null)
	{
		ArgumentNullException.ThrowIfNull(strategy);

		_strategy = strategy;
		_monitor = monitor;
	}

	public IArrangementStrategy Strategy => _strategy;
	public bool AllMonitors => _monitor is null;
	public Monitor? Monitor => _monitor;
}