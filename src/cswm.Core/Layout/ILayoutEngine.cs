using System.Collections.Generic;

namespace cswm.Core.Layout;

public interface ILayoutEngine
{
	/// <summary>
	/// Calculate a layout for a set of windows in a region.
	/// </summary>
	/// <param name="displayArea">Display region for the windows.</param>
	/// <param name="windows">Windows to layout.</param>
	/// <returns><see cref="LayoutResult"/> for the windows.</returns>
	LayoutResult CalculateLayout(Bounds displayArea, IReadOnlyList<WindowInfo> windows);

	/// <summary>
	/// Determine if a layout is valid, or needs re-calculating.
	/// </summary>
	/// <param name="displayArea">Display region for the windows.</param>
	/// <param name="layout">Layout to be validated.</param>
	/// <returns><c>true</c> if the layout is valid; <c>false</c>, otherwise.</returns>
	bool ValidateLayout(Bounds displayArea, LayoutResult layout);
}