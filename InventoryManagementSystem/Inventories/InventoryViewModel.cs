using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Inventories
{
    public partial class InventoryViewModel : ViewModel
    {
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly INavigationService navigation;
        private readonly IServiceProvider serviceProvider;

        public InventoryViewModel(
            IInventoryApplicationService inventoryApplicationService,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.navigation = navigationService;
            this.serviceProvider = serviceProvider;

            // 初期化処理
            LoadLocations(); // 保管場所取得を先に行う必要あり
            LoadLatestInventories();
        }

        public ReactiveProperty<string> Keyword { get; } = new ReactiveProperty<string>();
        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();
        public ReactiveCollection<InventoryListDisplayModel> Inventories { get; } = new ReactiveCollection<InventoryListDisplayModel>();

        public ReactiveCommand SearchCommand => new ReactiveCommand().WithSubscribe(Search);

        private void Search()
        {
            Inventories.Clear();
            var results = inventoryApplicationService.Find(Keyword.Value);

            foreach (var inventory in results)
            {
                Inventories.Add(InventoryListDisplayModel.FromInventory(inventory, Locations));
            }
        }

        public ReactiveProperty<InventoryListDisplayModel> SelectedInventory { get; } = new ReactiveProperty<InventoryListDisplayModel>();

        public ReactiveCommand ShowHistoryCommand => SelectedInventory
            .Select(x => x is not null)
            .ToReactiveCommand()
            .WithSubscribe(ShowHistory);

        private void ShowHistory()
        {
            if (SelectedInventory is null)
            {
                return;
            }

            // TODO: パラメーターを渡す方法がない...
            var viewModel = serviceProvider.GetRequiredService<InventoryTransactionViewModel>();
            viewModel.InventoryTransactionSearchViewModel.InventoryId.Value = SelectedInventory.Value.Id;
            viewModel.SearchCommand.Execute();
            navigation.Navigate(typeof(InventoryTransactionPage), viewModel);
        }

        public ReactiveCommand ShowStoreWithdrawCommand => SelectedInventory
            .Select(x => x is not null)
            .ToReactiveCommand()
            .WithSubscribe(ShowStoreWithdraw);

        private void ShowStoreWithdraw()
        {
            if (SelectedInventory is null)
            {
                return;
            }

            var viewModel = serviceProvider.GetRequiredService<StoreWithdrawViewModel>();
            viewModel.InventoryId.Value = SelectedInventory.Value.Id;
            navigation.Navigate(typeof(StoreWithdrawPage), viewModel);
        }

        private void LoadLocations()
        {
            var locations = inventoryApplicationService.FindAllLocation();
            Locations.Clear();
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private void LoadLatestInventories()
        {
            var latestInventories = inventoryApplicationService.GetLatest(30);
            Inventories.Clear();
            foreach (var inventories in latestInventories)
            {
                Inventories.Add(InventoryListDisplayModel.FromInventory(inventories, Locations));
            }
        }
    }
}