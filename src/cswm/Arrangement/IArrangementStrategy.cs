using cswm.WinApi;
using System.Drawing;

namespace cswm.Arrangement;

public interface IArrangementStrategy
{
    static string DisplayName { get; } = "[NOT SET]";

    MonitorLayout? Arrange(MonitorLayout prevLayout);

    MonitorLayout? AddWindow(MonitorLayout prevLayout, Window newWindow);

    MonitorLayout? MoveWindow(MonitorLayout prevLayout, Window movedWindow, Point cursorPosition);

    MonitorLayout? RemoveWindow(MonitorLayout prevLayout, Window removedWindow);

    void Reset();
}