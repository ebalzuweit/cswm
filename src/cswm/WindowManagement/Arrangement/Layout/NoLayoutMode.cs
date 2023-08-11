namespace cswm.WindowManagement.Arrangement.Layout;

public class NoLayoutMode : ILayoutMode
{
	public string DisplayName => "None";
	public void AddWindow(Window window) { }
	public void RemoveWindow(Window window) { }
	public void WindowMoved(Window window) { }
}
