using System.Drawing;

namespace cswm.WindowManagement.Arrangement;

public class SilentArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Silent";

    public MonitorLayout Arrange(MonitorLayout layout) => null!;

    public MonitorLayout ArrangeOnWindowMove(MonitorLayout layouts, Window movedWindow, Point cursorPosition) => null!;
}