using cswm.WinApi;
using System.Drawing;

namespace cswm.Arrangement;

public interface IArrangementStrategy
{
    static string DisplayName { get; } = null!;

    MonitorLayout Arrange(MonitorLayout layout);

    MonitorLayout ArrangeOnWindowMove(MonitorLayout layout, Window movedWindow, Point cursorPosition);

    void Reset();
}