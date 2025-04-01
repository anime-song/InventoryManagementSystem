using InventoryManagementSystem.Domain.Inventories;
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
    public partial class LocationRegisterViewModel : ViewModel
    {
        private readonly InventoryApplicationService inventoryApplicationService;
        private readonly ISnackbarService snackbarService;
        public LocationRegisterViewModel(
            InventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.snackbarService = snackbarService;

            Name.SetValidateAttribute(() => Name);

            RegisterCommand = new[] { Name.ObserveHasErrors, Description.ObserveHasErrors }
                .CombineLatestValuesAreAllFalse()
                .ToReactiveCommand()
                .WithSubscribe(Register);

            // 初期化処理
            LoadLocations();
        }

        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();

        [Required(ErrorMessage = "名称を入力してください", AllowEmptyStrings = false)]
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();

        public ReactiveCommand RegisterCommand { get; }

        private void Register()
        {
            RunWithErrorNotify(() =>
            {
                inventoryApplicationService.RegisterLocation(
                Name.Value,
                Description.Value);

                snackbarService.Show(
                    "登録完了",
                    "保管場所を登録しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));

                ClearInputValue();
                LoadLocations();
            });

        }

        private void ClearInputValue()
        {
            Name.Value = null;
            Description.Value = null;
        }

        private void LoadLocations()
        {
            Locations.Clear();
            var locations = inventoryApplicationService.FindAllLocation();
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }
    }
}