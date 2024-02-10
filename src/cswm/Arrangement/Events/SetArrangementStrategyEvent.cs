using cswm.Events;
using cswm.WinApi;
using System;

namespace cswm.Arrangement.Events;

/// <summary>
/// Sets the arrangement strategy for a specific, or all, Monitor(s).
/// </summary>
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

    /// <remarks>
    /// Not null if <see cref="AllMonitors"/> is <c>false</c>.
    /// </remarks>
    public Monitor? Monitor => _monitor;
}