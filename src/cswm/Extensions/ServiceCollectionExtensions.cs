using cswm.Arrangement;
using cswm.Events;
using cswm.Services.Arrangement;
using cswm.Services.SystemTray;
using cswm.Services.Tracking;
using cswm.Services.WinApi;
using cswm.Tracking;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCswmServices(this IServiceCollection services)
    {
        services.AddOptions<WindowManagementOptions>()
                .BindConfiguration(nameof(WindowManagementOptions))
                .ValidateOnStart();

        // Multiple services require the same instance of the following:
        services.AddSingleton<MessageBus>();
        services.AddSingleton<Win32RelayService>();
        services.AddSingleton<WindowTrackingService>();
        services.AddSingleton<WindowArrangementService>();
        services.AddSingleton<SystemTrayService>();
        services.AddSingleton<WindowManagementService>();

        // Should only be resolved by Program.cs
        services.AddScoped<Startup>();

        // Other registrations
        services.AddTransient<IWindowTrackingStrategy, DefaultWindowTrackingStrategy>();
        services.AddTransient<SplitArrangementStrategy>();
        services.AddTransient<SilentArrangementStrategy>();
        services.AddTransient<SystemTrayMenu>();
    }
}
