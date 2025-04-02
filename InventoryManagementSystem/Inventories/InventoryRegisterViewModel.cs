using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementSystem.Domain.Inventories;
using InventoryManagementSystem.WPF.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Inventories
{
    public partial class InventoryRegisterViewModel : ViewModel
    {
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly ISnackbarService snackbarService;

        public InventoryRegisterViewModel(
            IInventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.snackbarService = snackbarService;

            ItemName.SetValidateAttribute(() => ItemName);
            Quantity.SetValidateAttribute(() => Quantity);
            SelectedLocation.SetValidateAttribute(() => SelectedLocation);
            TransactionDate.SetValidateAttribute(() => TransactionDate);

            ExecuteCommand = new[] {
                ItemName.ObserveHasErrors,
                Quantity.ObserveHasErrors,
                SelectedLocation.ObserveHasErrors,
                TransactionDate.ObserveHasErrors }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(Execute);

            // 初期化処理
            LoadLocations(); // 保管場所を取得を先に呼ぶ必要あり
            LoadRecentInventories();
            ClearInputValue();
        }

        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();
        public ReactiveCollection<RecentInventoryDisplayModel> RecentInventories { get; } = new ReactiveCollection<RecentInventoryDisplayModel>();

        [Required(ErrorMessage = "商品名を入力してください", AllowEmptyStrings = false)]
        public ReactiveProperty<string> ItemName { get; } = new ReactiveProperty<string>();

        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上で入力してください")]
        [Required(ErrorMessage = "数量を入力してください")]
        public ReactiveProperty<int?> Quantity { get; } = new ReactiveProperty<int?>();

        [Required(ErrorMessage = "保管場所を選択してください")]
        public ReactiveProperty<Location> SelectedLocation { get; } = new ReactiveProperty<Location>();

        [Required(ErrorMessage = "日付を入力してください")]
        public ReactiveProperty<DateTime?> TransactionDate { get; } = new ReactiveProperty<DateTime?>(DateTime.Today);

        public ReactiveCommand ExecuteCommand { get; }

        private void Execute()
        {
            RunWithErrorNotify(() =>
            {
                var registeredInventory = inventoryApplicationService.Register(
                    itemName: ItemName.Value,
                    quantity: 0,
                    locationId: SelectedLocation.Value.Id!.Value);
                inventoryApplicationService.Store(
                    inventoryId: registeredInventory.Id!.Value,
                    quantity: Quantity.Value!.Value,
                    storeDate: TransactionDate.Value!.Value,
                    sourceType: TransactionSourceType.Manual,
                    sourceId: null);

                snackbarService.Show(
                    "登録完了",
                    "在庫を登録しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));

                LoadRecentInventories();
                ClearInputValue();
            });
        }

        private void ClearInputValue()
        {
            ItemName.Value = null;
            Quantity.Value = null;
            TransactionDate.Value = DateTime.Today;
            SelectedLocation.Value = null;
        }

        /// <summary>
        /// 直近の在庫を取得します
        /// </summary>
        private void LoadRecentInventories()
        {
            var recentItems = inventoryApplicationService.GetLatest(5);
            RecentInventories.Clear();
            foreach (var item in recentItems)
            {
                RecentInventories.Add(RecentInventoryDisplayModel.FromInventory(item, Locations));
            }
        }

        /// <summary>
        /// すべての保管場所を取得します
        /// </summary>
        private void LoadLocations()
        {
            var locations = inventoryApplicationService.FindAllLocation();
            Locations.Clear();
            foreach (var item in locations)
            {
                Locations.Add(item);
            }
        }
    }
}