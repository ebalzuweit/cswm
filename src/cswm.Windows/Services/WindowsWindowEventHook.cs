using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Windows.Services;

public class WindowsWindowEventHook : IWindowEventHook
{
	private bool disposedValue;

	public event EventHandler<WindowInfo>? WindowCreated;
	public event EventHandler<WindowInfo>? WindowDestroyed;
	public event EventHandler<WindowInfo>? WindowMoved;
	public event EventHandler<WindowInfo>? WindowStateChanged;

	public void StartSubscriptions()
	{
		throw new NotImplementedException();
	}

	public void StopSubscriptions()
	{
		throw new NotImplementedException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~WindowsWindowEventHook()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
