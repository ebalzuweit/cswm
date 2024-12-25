using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Core.Test.Services;

public class MockWindowEventHook : IWindowEventHook
{
	private bool disposedValue;

	private bool running = false;

	public event EventHandler<WindowInfo>? WindowCreated;
	public event EventHandler<WindowInfo>? WindowDestroyed;
	public event EventHandler<WindowInfo>? WindowMoved;
	public event EventHandler<WindowInfo>? WindowStateChanged;

	public void StartSubscriptions()
	{
		running = true;
	}

	public void StopSubscriptions()
	{
		running = false;
	}

	public void InvokeWindowCreated(WindowInfo window)
	{
		ThrowIfNotRunning();

		WindowCreated?.Invoke(this, window);
	}

	public void InvokeWindowDestroyed(WindowInfo window)
	{
		ThrowIfNotRunning();

		WindowDestroyed?.Invoke(this, window);
	}

	public void InvokeWindowMoved(WindowInfo window)
	{
		ThrowIfNotRunning();

		WindowMoved?.Invoke(this, window);
	}

	public void InvokeWindowStateChanged(WindowInfo window)
	{
		ThrowIfNotRunning();

		WindowStateChanged?.Invoke(this, window);
	}

	private void ThrowIfNotRunning()
	{
		if (running == false)
		{
			throw new InvalidOperationException();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		running = false;

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
	// ~MockWindowEventHook()
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
