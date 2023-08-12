using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public interface IArrangementStrategy
{
    static string DisplayName { get; }

    IEnumerable<WindowLayout> Arrange(IEnumerable<MonitorLayout> layouts);

    IEnumerable<WindowLayout> ArrangeOnWindowMove(IEnumerable<MonitorLayout> layouts, Window movedWindow);
}