using cswm.WinApi;
using System.Drawing;

namespace cswm.Arrangement;

public class SilentArrangementStrategy : IArrangementStrategy
{
    public static string DisplayName => "Silent";

    public MonitorLayout? Arrange(MonitorLayout prevLayout) => default;
    public MonitorLayout? AddWindow(MonitorLayout prevLayout, Window newWindow) => default;
    public MonitorLayout? MoveWindow(MonitorLayout prevLayout, Window movedWindow, Point cursorPosition) => default;
    public MonitorLayout? RemoveWindow(MonitorLayout prevLayout, Window removedWindow) => default;
    public void Reset() { }
}