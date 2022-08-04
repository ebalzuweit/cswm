using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public interface IArrangementStrategy
{
    WindowArrangement Arrange(ICollection<Window> windows);
}