using System.Collections.Generic;
using System.Linq;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
    public IEnumerable<WindowLayout> Arrange(IEnumerable<WindowLayout> layouts)
    {
        return layouts.Select(layout => new WindowLayout(layout.Window, layout.Position, layout.Monitor));
    }
}