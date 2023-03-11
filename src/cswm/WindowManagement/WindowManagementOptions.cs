namespace cswm.WindowManagement;

public class WindowManagementOptions
{
    /// <summary>
    /// If set, windows will be tracked but not managed.
    /// </summary>
    public bool DoNotManage { get; init; }
    
    /// <summary>
    /// Margin around all windows.
    /// </summary>
    public int Margin { get; init; }

    /// <summary>
    /// Padding for each monitor.
    /// </summary>
    public int Padding { get; init; }
}