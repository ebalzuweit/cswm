namespace cswm.App.Services;

public interface IService
{
    /// <summary>
    /// Start the service
    /// </summary>
    public void Start();

    /// <summary>
    /// Stop the service
    /// </summary>
    public void Stop();
}