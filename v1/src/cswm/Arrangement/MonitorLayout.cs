using cswm.WinApi;
using System.Collections.Generic;

namespace cswm.Arrangement;

public record MonitorLayout(Monitor Monitor, IEnumerable<WindowLayout> Windows);