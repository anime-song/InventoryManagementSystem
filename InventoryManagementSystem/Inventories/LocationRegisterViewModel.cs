using InventoryManagementSystem.Domain.Applications.Inventories;
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
    public partial class LocationRegisterViewModel : ViewModel
    {
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly ISnackbarService snackbarService;

        public LocationRegisterViewModel(
            IInventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;
            this.snackbarService = snackbarService;

            Name.SetValidateAttribute(() => Name);

            RegisterCommand = new[]
            {
                Name.ObserveHasErrors,
                Description.ObserveHasErrors,
                IsEditMode.Select(x => x)
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand()
            .WithSubscribe(Register);

            EditCommand = new[]
            {
                Name.ObserveHasErrors,
                Description.ObserveHasErrors,
                IsEditMode.Select(x => x).Inverse()
            }
            .CombineLatestValuesAreAllFalse()
                .ToReactiveCommand()
                .WithSubscribe(Edit);

            CancelCommand = IsEditMode
                .Select(x => x)
                .ToReactiveCommand()
                .WithSubscribe(ChangeRegisterMode);

            ChangeEditModeCommand = IsEditMode
                .Select(x => !x)
                .ToReactiveCommand<Location>()
                .WithSubscribe(ChangeEditMode);

            ModeLabel = IsEditMode
                .Select(x => x ? $"保管場所編集 (ID: {EditingLocation.Value?.Id})" : "保管場所登録")
                .ToReadOnlyReactivePropertySlim<string>();

            // 初期化処理
            LoadLocations();
        }

        public ReactiveProperty<bool> IsEditMode { get; } = new ReactiveProperty<bool>();
        public ReadOnlyReactivePropertySlim<string> ModeLabel { get; }
        public ReactiveProperty<Location> EditingLocation { get; } = new ReactiveProperty<Location>();
        public ReactiveCollection<Location> Locations { get; } = new ReactiveCollection<Location>();

        [Required(ErrorMessage = "名称を入力してください", AllowEmptyStrings = false)]
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();

        public ReactiveCommand RegisterCommand { get; }
        public ReactiveCommand EditCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand<Location> ChangeEditModeCommand { get; }

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

        private void Edit()
        {
            RunWithErrorNotify(() =>
            {
                inventoryApplicationService.UpdateLocation(
                    EditingLocation.Value.Id!.Value,
                    Name.Value,
                    Description.Value);

                snackbarService.Show(
                    "登録完了",
                    "保管場所を更新しました",
                    Wpf.Ui.Controls.ControlAppearance.Success,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));
                ChangeRegisterMode();
                ClearInputValue();
                LoadLocations();
            });
        }

        private void ChangeEditMode(Location location)
        {
            EditingLocation.Value = location;
            IsEditMode.Value = true;
            Name.Value = location.Name;
            Description.Value = location.Description;
        }

        private void ChangeRegisterMode()
        {
            IsEditMode.Value = false;
            ClearInputValue();
        }

        private void ClearInputValue()
        {
            EditingLocation.Value = null;
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