using System.Collections.Generic;
using System.Linq;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
    public WindowArrangement Arrange(ICollection<Window> windows)
    {
        return new WindowArrangement(windows, windows.Select(w => new Rect()).ToArray());
    }
}