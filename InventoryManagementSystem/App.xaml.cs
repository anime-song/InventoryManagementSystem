using InventoryManagementSystem.Domain.Inventories;
using InventoryManagementSystem.Infra.Inventories;
using InventoryManagementSystem.WPF.Inventories;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace InventoryManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IServiceCollection services = new ServiceCollection();
            services.AddNavigationViewPageProvider();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<LiteDatabase>(new LiteDatabase("./inventory.db"));
            services.AddSingleton<IInventoryRepository, InventoryRepository>();
            services.AddSingleton<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddSingleton<ILocationRepository, LocationRepository>();
            services.AddSingleton<IInventoryApplicationService, InventoryApplicationService>();

            services.AddTransient<InventoryViewModel>();
            services.AddTransient<InventoryPage>();
            services.AddTransient<InventoryRegisterViewModel>();
            services.AddTransient<InventoryRegisterPage>();
            services.AddTransient<LocationRegisterViewModel>();
            services.AddTransient<LocationRegisterPage>();
            services.AddTransient<InventoryTransactionViewModel>();
            services.AddTransient<InventoryTransactionPage>();
            services.AddTransient<StoreWithdrawViewModel>();
            services.AddTransient<StoreWithdrawPage>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ISnackbarService, SnackbarService>(); // スナックバー表示用

            Services = services.BuildServiceProvider();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}