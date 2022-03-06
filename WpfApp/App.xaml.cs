using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Bootstrap.Bootstrap _bootstrap;

        public App()
        {
            _bootstrap = new Bootstrap.Bootstrap();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _bootstrap.StartAsync();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _bootstrap.StopAsync();
            base.OnExit(e);
        }
    }
}