using System.Collections.Generic;
using System.Linq;

namespace cswm.Core.Layout;

public record class LayoutResult
(
	IList<WindowLayout> WindowLayouts
)
{
	public static LayoutResult Merge(LayoutResult a, LayoutResult b)
		=> new(a.WindowLayouts.Concat(b.WindowLayouts).ToArray());

	public static LayoutResult Merge(WindowLayout a, LayoutResult b)
		=> new(b.WindowLayouts.Prepend(a).ToArray());

	public static LayoutResult Merge(LayoutResult a, WindowLayout b)
		=> new(a.WindowLayouts.Append(b).ToArray());
}
