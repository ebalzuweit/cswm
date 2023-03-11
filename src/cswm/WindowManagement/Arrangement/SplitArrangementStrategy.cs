using System;
using System.Collections.Generic;
using System.Linq;
using cswm.WinApi;
using Microsoft.Extensions.Options;

namespace cswm.WindowManagement.Arrangement;

public class SplitArrangementStrategy : IArrangementStrategy
{
	private readonly WindowManagementOptions _options;

    public SplitArrangementStrategy(IOptions<WindowManagementOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public IEnumerable<WindowLayout> Arrange(IEnumerable<MonitorLayout> monitorLayouts)
	{
		return monitorLayouts.SelectMany(monitorLayout =>
			PartitionSpace(monitorLayout.Space.AddMargin(_options.Padding), monitorLayout.Windows));
	}

	private IEnumerable<WindowLayout> PartitionSpace(Rect space, IEnumerable<WindowLayout> windows)
	{
		if (windows.Count() == 0)
			return Array.Empty<WindowLayout>();
		if (windows.Count() == 1)
		{
			return new[] {
				new WindowLayout(windows.First().hWnd, space.AddMargin(_options.Margin))
			};
		}

		var (left, right, verticalSplit) = space.Split();
		// TODO: pick window partitions better - should try to preserve current position / dimensions
		var leftSpace = verticalSplit switch
		{
			true => left.AddMargin(_options.Margin, _options.Margin, 0, _options.Margin),
			false => left.AddMargin(_options.Margin, _options.Margin, _options.Margin, 0)
		};
        var leftPartition = new WindowLayout(windows.First().hWnd, leftSpace);
        var layouts = PartitionSpace(right, windows.Skip(1));
		layouts = layouts.Prepend(leftPartition);
		return layouts;
	}
}