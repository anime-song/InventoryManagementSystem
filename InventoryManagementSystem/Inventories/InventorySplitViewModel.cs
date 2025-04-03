using DynamicData;
using DynamicData.Binding;
using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Inventories.Requests;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.WPF.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed class SplitItemModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "商品名を入力してください")]
        public ReactiveProperty<string> ItemName { get; } = new ReactiveProperty<string>();

        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上で入力してください")]
        [Required(ErrorMessage = "数量を入力してください")]
        public ReactiveProperty<int?> Quantity { get; } = new ReactiveProperty<int?>();

        [Required(ErrorMessage = "保管場所を選択してください")]
        public ReactiveProperty<int?> LocationId { get; } = new ReactiveProperty<int?>();

        public IObservable<bool> ObserveIsValid { get; }

        public SplitItemModel()
        {
            ItemName.SetValidateAttribute(() => ItemName);
            Quantity.SetValidateAttribute(() => Quantity);
            LocationId.SetValidateAttribute(() => LocationId);

            ObserveIsValid = new[]
            {
                ItemName.ObserveHasErrors,
                Quantity.ObserveHasErrors,
                LocationId.ObserveHasErrors,
            }.CombineLatestValuesAreAllFalse();
        }

        public SplitInventoryRequest.SplitInventoryItemRequest ToRequestItem()
        {
            return new(itemName: ItemName.Value, quantity: Quantity.Value!.Value, locationId: LocationId.Value!.Value);
        }
    }

    public partial class InventorySplitViewModel : ViewModel
    {
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly ISnackbarService snackbarService;

        public InventorySplitViewModel(
            IInventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.snackbarService = snackbarService;

            SearchInventoryCommand = SearchInventoryId
                .Select(x => x is not null)
                .ToReactiveCommand()
                .WithSubscribe(Search);
            AddSplitItemCommand = new ReactiveCommand().WithSubscribe(AddSplitItem);
            RemoveSplitItemCommand = new ReactiveCommand<SplitItemModel>().WithSubscribe(RemoveSplitItem);

            ExecuteSplitCommand = SplitItems
                .ToObservableChangeSet()
                .AutoRefreshOnObservable(x => x.ObserveIsValid)
                .ToCollection()
                .Select(items =>
                {
                    if (items.Count == 0) return Observable.Return(false);
                    return items
                        .Select(x => x.ObserveIsValid)
                        .CombineLatest()
                        .Select(x => x.All(y => y));
                })
                .Switch()
                .ToReactiveCommand()
                .WithSubscribe(ExecuteSplit);

            IsInventoryLoaded = SelectedInventory.Select(x => x is not null).ToReadOnlyReactivePropertySlim();
            // 初期化処理
            LoadLocations();
        }

        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();
        public ReactiveProperty<int?> SearchInventoryId { get; } = new ReactiveProperty<int?>();
        public ReadOnlyReactivePropertySlim<bool> IsInventoryLoaded { get; }
        public ReactiveProperty<InventoryListDisplayModel> SelectedInventory { get; } = new ReactiveProperty<InventoryListDisplayModel>();

        public ReactiveCollection<SplitItemModel> SplitItems { get; } = new ReactiveCollection<SplitItemModel>();
        public ReactiveCommand SearchInventoryCommand { get; }
        public ReactiveCommand AddSplitItemCommand { get; }
        public ReactiveCommand<SplitItemModel> RemoveSplitItemCommand { get; }
        public ReactiveCommand ExecuteSplitCommand { get; }

        private void Search()
        {
            RunWithErrorNotify(() =>
            {
                SelectedInventory.Value = null;

                var inventory = inventoryApplicationService.FindById(SearchInventoryId.Value!.Value);
                SelectedInventory.Value = InventoryListDisplayModel.FromInventory(inventory, Locations);
            });
        }

        private void AddSplitItem()
        {
            RunWithErrorNotify(() =>
            {
                SplitItems.Add(new SplitItemModel());
            });
        }

        private void RemoveSplitItem(SplitItemModel model)
        {
            RunWithErrorNotify(() =>
            {
                SplitItems.Remove(model);
            });
        }

        private void ExecuteSplit()
        {
            RunWithErrorNotify(() =>
            {
                inventoryApplicationService.SplitInventory(new SplitInventoryRequest(
                    sourceInventoryId: SelectedInventory.Value.Id,
                    items: SplitItems.Select(x => x.ToRequestItem())));

                snackbarService.Show(
                    "登録完了",
                    "在庫を分割しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));

                ClearInputValue();
            });
        }

        private void ClearInputValue()
        {
            SplitItems.Clear();
            SelectedInventory.Value = null;
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