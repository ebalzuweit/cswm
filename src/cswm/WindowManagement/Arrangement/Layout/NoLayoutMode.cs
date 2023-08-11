using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement.Layout;

public class NoLayoutMode : ILayoutMode
{
	public string DisplayName => "None";

	public void Initialize(IEnumerable<Window> windows) { }
	public void AddWindow(Window window) { }
	public void RemoveWindow(Window window) { }
	public void WindowMoved(Window window) { }
}
