using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement.Layout;

public sealed class FixedHierarchyLayoutMode : ILayoutMode
{
	public string DisplayName => "One, two, 3";

	public void Initialize(IEnumerable<Window> windows)
	{
		/*
		* TODO: Implement basic 'One, two, 3' layout
		*
		* Layout windows 1/2, 1/3, 1/6 of desktop
		*/
	}

	public void AddWindow(Window window)
	{

	}

	public void RemoveWindow(Window window)
	{

	}

	public void WindowMoved(Window window)
	{

	}
}