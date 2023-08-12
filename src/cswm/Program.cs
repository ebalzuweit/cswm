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
            services.AddSingleton<WindowLayoutService>();
            services.AddSingleton<WindowManagementService>();
            services.AddSingleton<SystemTrayService>();

            // Should only be resolved by this class
            services.AddScoped<Startup>();

            // Other registrations
            services.AddTransient<IWindowTrackingStrategy, DefaultWindowTrackingStrategy>();
            services.AddTransient<SplitArrangementStrategy>();
            services.AddTransient<SilentArrangementStrategy>();
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
