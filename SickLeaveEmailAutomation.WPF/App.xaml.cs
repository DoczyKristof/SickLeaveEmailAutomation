using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SickLeaveEmailAutomation.WPF.View;
using SickLeaveEmailAutomation.WPF.ViewModel;
using System;
using System.Windows;

namespace SickLeaveEmailAutomation.WPF
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        private IServiceProvider _serviceProvider;

        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<App>();

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddTransient<MainWindow>();
            services.AddTransient<ScanViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
