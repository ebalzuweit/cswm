using cswm.WinApi;
using System.Drawing;

namespace cswm.Arrangement;

public class SilentArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Silent";

    public MonitorLayout Arrange(MonitorLayout layout) => null!;

    public MonitorLayout ArrangeOnWindowMove(MonitorLayout layouts, Window movedWindow, Point cursorPosition) => null!;

    public void Reset() { }
}