using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.WPF.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed class StoreWithdrawViewModel : ViewModel
    {
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly ISnackbarService snackbarService;

        public StoreWithdrawViewModel(
            IInventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.snackbarService = snackbarService;
            this.inventoryApplicationService = inventoryApplicationService;

            InventoryId.SetValidateAttribute(() => InventoryId);
            Quantity.SetValidateAttribute(() => Quantity);
            TransactionDate.SetValidateAttribute(() => TransactionDate);
            SelectedTransactionType.SetValidateAttribute(() => SelectedTransactionType);

            ExecuteCommand = new[] {
                InventoryId.ObserveHasErrors,
                Quantity.ObserveHasErrors,
                TransactionDate.ObserveHasErrors,
                SelectedTransactionType.ObserveHasErrors
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(Execute);

            // 初期化処理
            LoadLocations(); // 保管場所を取得を先に呼ぶ必要あり
            LoadRecentInventories();
        }

        public IReadOnlyList<TransactionType> TransactionTypes => TransactionType.Items.Where(x => x != TransactionType.Cancel).ToList();
        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();
        public ReactiveCollection<RecentInventoryDisplayModel> RecentInventories { get; } = new ReactiveCollection<RecentInventoryDisplayModel>();

        [Required(ErrorMessage = "在庫IDを入力してください", AllowEmptyStrings = false)]
        public ReactiveProperty<int?> InventoryId { get; } = new ReactiveProperty<int?>();

        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上で入力してください")]
        [Required(ErrorMessage = "数量を入力してください")]
        public ReactiveProperty<int?> Quantity { get; } = new ReactiveProperty<int?>();

        [Required(ErrorMessage = "入庫・出庫区分を入力してください")]
        public ReactiveProperty<TransactionType> SelectedTransactionType { get; } = new ReactiveProperty<TransactionType>();

        [Required(ErrorMessage = "日付を入力してください")]
        public ReactiveProperty<DateTime?> TransactionDate { get; } = new ReactiveProperty<DateTime?>(DateTime.Today);

        public ReactiveCommand ExecuteCommand { get; }

        private void Execute()
        {
            RunWithErrorNotify(() =>
            {
                if (SelectedTransactionType.Value == TransactionType.In)
                {
                    inventoryApplicationService.Store(
                        inventoryId: InventoryId!.Value!.Value,
                        quantity: Quantity.Value!.Value,
                        storeDate: TransactionDate.Value!.Value,
                        sourceType: TransactionSourceType.Manual,
                        sourceId: null);
                }
                else if (SelectedTransactionType.Value == TransactionType.Out)
                {
                    inventoryApplicationService.Withdraw(
                        inventoryId: InventoryId.Value!.Value,
                        quantity: Quantity.Value!.Value,
                        withdrawDate: TransactionDate.Value!.Value,
                        sourceType: TransactionSourceType.Manual,
                        sourceId: null);
                }

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
            InventoryId.Value = null;
            Quantity.Value = null;
            TransactionDate.Value = DateTime.Today;
            SelectedTransactionType.Value = null;
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