using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public interface IArrangementStrategy
{
    static string DisplayName { get; } = null!;

    IEnumerable<MonitorLayout> Arrange(IEnumerable<MonitorLayout> layouts);

    IEnumerable<MonitorLayout> ArrangeOnWindowMove(IEnumerable<MonitorLayout> layouts, Window movedWindow);
}