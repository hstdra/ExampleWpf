﻿using Autofac;
using WpfApp.Commands;
using WpfApp.Services;
using WpfApp.Stores;
using WpfApp.ViewModels;
using WpfApp.Views.Windows;

namespace WpfApp.Bootstrap;

public static class ContainerBuilderExtensions
{
    public static void Setup(this ContainerBuilder builder)
    {
        builder
            .AddStores()
            .AddServices()
            .AddCommands()
            .AddViews();
    }

    private static ContainerBuilder AddStores(this ContainerBuilder builder)
    {
        builder.RegisterType<Store>().SingleInstance();
        
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
}