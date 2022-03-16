using Autofac;
using Mapster;
using Microsoft.Extensions.Hosting;
using WpfApp.Commands;
using WpfApp.Services;
using WpfApp.Stores;
using WpfApp.ViewModels;
using WpfApp.Views.Windows;
using WpfApp.Workers;

namespace WpfApp.Bootstrap;

public static class ContainerBuilderExtensions
{
    public static void Setup(this ContainerBuilder builder)
    {
        builder
            .AddWorkers()
            .AddStores()
            .AddServices()
            .AddCommands()
            .AddViews();
    }

    private static ContainerBuilder AddStores(this ContainerBuilder builder)
    {
        var savedRootStore = StoreManagerWorker.GetSaveRootStore();
        var rootStore = new RootStore();
        var productStore = savedRootStore.ProductStore is null ? new ProductStore(rootStore) : savedRootStore.ProductStore.Adapt(new ProductStore(rootStore));
        
        builder.Register((_) => rootStore).SingleInstance();
        builder.Register((_) => productStore).SingleInstance();
        
        return builder;
    }

    private static ContainerBuilder AddViews(this ContainerBuilder builder)
    {
        builder.RegisterType<DashboardWindowViewModel>().SingleInstance();
        builder.RegisterType<DashboardWindow>().SingleInstance();
        
        return builder;
    }

    private static ContainerBuilder AddServices(this ContainerBuilder builder)
    {
        builder.RegisterType<ProductService>().As<IProductService>();

        return builder;
    }

    private static ContainerBuilder AddCommands(this ContainerBuilder builder)
    {
        return builder;
    }

    private static ContainerBuilder AddWorkers(this ContainerBuilder builder)
    {
        builder.RegisterType<StoreManagerWorker>().As<IHostedService>();

        return builder;
    }
}