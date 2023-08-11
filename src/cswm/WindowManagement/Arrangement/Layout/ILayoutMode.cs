using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement.Layout;

public interface ILayoutMode
{
	public string DisplayName { get; }

	public void Initialize(IEnumerable<Window> windows);
	public void AddWindow(Window window);
	public void RemoveWindow(Window window);
	public void WindowMoved(Window window);
}