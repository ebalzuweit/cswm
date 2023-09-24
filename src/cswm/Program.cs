﻿using cswm.Events;
using cswm.WindowManagement;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Services;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace cswm;

/// <summary>
/// Build services and start the application.
/// </summary>
internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        using var host = BuildHost(args);
        using var scope = host.Services.CreateScope();
        var startup = scope.ServiceProvider.GetRequiredService<Startup>();

        host.Start();
        startup.Start();
    }

    private static IHost BuildHost(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices((_, services) =>
        {
            services.AddOptions<WindowManagementOptions>()
                .BindConfiguration(nameof(WindowManagementOptions))
                .ValidateOnStart();

            // Multiple services require the same instance of the following:
            services.AddSingleton<MessageBus>();
            services.AddSingleton<WinHookService>();
            services.AddSingleton<WindowTrackingService>();
            services.AddSingleton<WindowArrangementService>();
            services.AddSingleton<SystemTrayService>();
            services.AddSingleton<WindowManagementService>();

            // Should only be resolved by this class
            services.AddScoped<Startup>();

            // Other registrations
            services.AddTransient<IWindowTrackingStrategy, DefaultWindowTrackingStrategy>();
            services.AddTransient<SplitArrangementStrategy>();
            services.AddTransient<SilentArrangementStrategy>();
            services.AddTransient<SystemTrayMenu>();
        });
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
#if DEBUG
            logging.AddDebug();
#endif
        });
        return builder.Build();
    }
}
