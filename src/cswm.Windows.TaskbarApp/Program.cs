using cswm.Windows;
using cswm.Windows.TaskbarApp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCswmWindowsServices();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
