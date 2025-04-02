using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Purchases;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.Domain.Domains.Purchases;
using InventoryManagementSystem.Domain.Queries.Purchases;
using InventoryManagementSystem.Infra.Inventories;
using InventoryManagementSystem.Infra.Purchases;
using InventoryManagementSystem.Infra.Queries;
using InventoryManagementSystem.WPF.Inventories;
using InventoryManagementSystem.WPF.Purchases;
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
            services.AddSingleton<IPurchaseRepository, PurchaseRepository>();
            services.AddSingleton<IPurchaseApplicationService, PurchaseApplicationService>();

            services.AddSingleton<IPurchaseQueryService, PurchaseQueryService>();

            services.AddTransient<InventoryViewModel>();
            services.AddTransient<InventoryPage>();
            services.AddTransient<LocationRegisterViewModel>();
            services.AddTransient<LocationRegisterPage>();
            services.AddTransient<InventoryTransactionViewModel>();
            services.AddTransient<InventoryTransactionPage>();
            services.AddTransient<StoreWithdrawViewModel>();
            services.AddTransient<StoreWithdrawPage>();
            services.AddTransient<PurchaseRegisterViewModel>();
            services.AddTransient<PurchaseRegisterPage>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ISnackbarService, SnackbarService>(); // スナックバー表示用

            Services = services.BuildServiceProvider();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}