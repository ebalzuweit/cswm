using System;
using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public class SilentArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Silent";

    public IEnumerable<MonitorLayout> Arrange(IEnumerable<MonitorLayout> layouts)
        => Array.Empty<MonitorLayout>();

    public IEnumerable<MonitorLayout> ArrangeOnWindowMove(IEnumerable<MonitorLayout> layouts, Window movedWindow)
        => Array.Empty<MonitorLayout>();
}