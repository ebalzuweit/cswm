using System;
using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement.Layout;

public class NoLayoutMode : ILayoutMode
{
	public void AddWindow(Window window) { }
	public void RemoveWindow(IntPtr hWnd) { }
	public IEnumerable<Window> GetWindows() => Array.Empty<Window>();
}