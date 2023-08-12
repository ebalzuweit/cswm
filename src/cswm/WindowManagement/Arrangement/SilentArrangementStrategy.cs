using System;
using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public class SilentArrangementStrategy : IArrangementStrategy
{
	public static string DisplayName => "Silent";

	public IEnumerable<WindowLayout> Arrange(IEnumerable<MonitorLayout> layouts)
		=> Array.Empty<WindowLayout>();

	public IEnumerable<WindowLayout> ArrangeOnWindowMove(IEnumerable<MonitorLayout> layouts, Window movedWindow)
		=> Array.Empty<WindowLayout>();
}