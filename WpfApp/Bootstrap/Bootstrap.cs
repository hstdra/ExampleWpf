using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WpfApp.Views.Windows;

namespace WpfApp.Bootstrap;

public class Bootstrap
{
    private readonly IHost _host;

    public Bootstrap()
    {
        _host = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .UseSerilog((_, loggerConfiguration) =>
            {
                loggerConfiguration.WriteTo.File("AppLog.txt", rollingInterval: RollingInterval.Day)
                    .WriteTo.Debug()
                    .WriteTo.Console();
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<StoreManager>();
                services.AddMediatR(Assembly.GetExecutingAssembly());
            })
            .ConfigureContainer<ContainerBuilder>(builder => builder.Setup())
            .Build();

        ConsoleAllocator.ShowConsoleWindow();
        ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(_host.Services.GetAutofacRoot()));
    }

    public async Task StartAsync()
    {
        try
        {
            await _host.StartAsync();
            var window = _host.Services.GetRequiredService<DashboardWindow>();
            window.Show();
        }
        catch (Exception exception)
        {
            var logger = _host.Services.GetRequiredService<ILogger<Bootstrap>>();
            logger.LogCritical("Critical error, shutting down! {Exception}", exception);
            throw;
        }
    }

    public async Task StopAsync()
    {
        Console.ReadLine();
        await _host.StopAsync();
        _host.Dispose();
    }
}