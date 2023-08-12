using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public record MonitorLayout(Monitor Monitor, IEnumerable<WindowLayout> Windows);