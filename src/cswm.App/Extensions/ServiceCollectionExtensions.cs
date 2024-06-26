﻿using cswm.App.Services;
using cswm.App.Ux;
using cswm.Arrangement;
using cswm.Events;
using cswm.Options;
using cswm.Tracking;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.App;

public static class ServiceCollectionExtensions
{
    public static void AddCswmServices(this IServiceCollection services)
    {
        services.AddOptions<WindowManagementOptions>()
                .BindConfiguration(nameof(WindowManagementOptions))
                .ValidateOnStart();

        // Multiple services require the same instance of the following:
        services.AddSingleton<MessageBus>();
        services.AddSingleton<WindowEventRelayService>();
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
