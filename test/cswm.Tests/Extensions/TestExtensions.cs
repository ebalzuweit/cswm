using cswm.Arrangement;
using cswm.WinApi;
using System.Drawing;
using System.Linq;

namespace cswm.Tests;

public static class TestExtensions
{
    public static WindowLayout GetWindowByClassName(this MonitorLayout layout, string className)
        => layout.Windows.First(x => x.Window.ClassName == className);

    public static Rect MoveTo(this Rect initial, Point position)
        => new(position.X, position.Y, initial.Width, initial.Height);
}
