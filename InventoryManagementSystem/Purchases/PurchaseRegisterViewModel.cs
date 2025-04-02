using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Purchases;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.Domain.Queries.Purchases;
using InventoryManagementSystem.WPF.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Purchases
{
    public partial class PurchaseRegisterViewModel : ViewModel
    {
        private readonly ISnackbarService snackbarService;
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly IPurchaseApplicationService purchaseApplicationService;
        private readonly IPurchaseQueryService purchaseQueryService;

        public PurchaseRegisterViewModel(
            IInventoryApplicationService inventoryApplicationService,
            IPurchaseApplicationService purchaseApplicationService,
            IPurchaseQueryService purchaseQueryService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.purchaseApplicationService = purchaseApplicationService;
            this.purchaseQueryService = purchaseQueryService;
            this.snackbarService = snackbarService;

            ItemName.SetValidateAttribute(() => ItemName);
            Quantity.SetValidateAttribute(() => Quantity);
            SelectedLocation.SetValidateAttribute(() => SelectedLocation);
            PurchaseDate.SetValidateAttribute(() => PurchaseDate);

            ExecuteCommand = new[] {
                ItemName.ObserveHasErrors,
                Quantity.ObserveHasErrors,
                SelectedLocation.ObserveHasErrors,
                PurchaseDate.ObserveHasErrors
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(Execute);

            CancelPurchaseCommand = new ReactiveCommand<PurchaseInventoryDTO>()
                .WithSubscribe(CancelPurchase);

            // 初期化処理
            LoadLocations();
            LoadRecentPurchases();
            ClearInputValue();
        }

        public ReactiveCollection<PurchaseInventoryDTO> RecentPurchases { get; } = new ReactiveCollection<PurchaseInventoryDTO>();

        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();

        [Required(ErrorMessage = "商品名を入力してください", AllowEmptyStrings = false)]
        public ReactiveProperty<string> ItemName { get; } = new ReactiveProperty<string>();

        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上で入力してください")]
        [Required(ErrorMessage = "数量を入力してください")]
        public ReactiveProperty<int?> Quantity { get; } = new ReactiveProperty<int?>();

        [Required(ErrorMessage = "保管場所を選択してください")]
        public ReactiveProperty<Location> SelectedLocation { get; } = new ReactiveProperty<Location>();

        [Required(ErrorMessage = "日付を入力してください")]
        public ReactiveProperty<DateTime?> PurchaseDate { get; } = new ReactiveProperty<DateTime?>(DateTime.Today);

        public ReactiveCommand ExecuteCommand { get; }
        public ReactiveCommand<PurchaseInventoryDTO> CancelPurchaseCommand { get; }

        private void Execute()
        {
            RunWithErrorNotify(() =>
            {
                purchaseApplicationService.RegisterPurchase(
                    itemName: ItemName.Value,
                    purchaseDate: PurchaseDate.Value!.Value,
                    quantity: Quantity.Value!.Value,
                    locationId: SelectedLocation.Value.Id!.Value);

                snackbarService.Show(
                    "登録完了",
                    "仕入を登録しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));

                LoadRecentPurchases();
                ClearInputValue();
            });
        }

        private void CancelPurchase(PurchaseInventoryDTO purchase)
        {
            RunWithErrorNotify(() =>
            {
                purchaseApplicationService.CancelPurchase(purchase.PurchaseId);

                snackbarService.Show(
                    "登録完了",
                    "仕入を取消しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));

                LoadRecentPurchases();
            });
        }

        private void ClearInputValue()
        {
            ItemName.Value = null;
            Quantity.Value = null;
            PurchaseDate.Value = DateTime.Today;
            SelectedLocation.Value = null;
        }

        private void LoadRecentPurchases()
        {
            RecentPurchases.Clear();
            var purchases = purchaseQueryService.GetLatest(5);
            foreach (var purchase in purchases)
            {
                RecentPurchases.Add(purchase);
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