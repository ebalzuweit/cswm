namespace cswm.WindowManagement.Arrangement;

public interface IArrangementStrategy
{
    static string DisplayName { get; } = null!;

    MonitorLayout Arrange(MonitorLayout layout);

    MonitorLayout ArrangeOnWindowMove(MonitorLayout layout, Window movedWindow);
}