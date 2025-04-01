using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace InventoryManagementSystem.WPF.ViewModels
{
    public abstract class ViewModel : INavigationAware
    {
        public virtual Task OnNavigatedFromAsync()
        {
            OnNavigatedFrom();

            return Task.CompletedTask;
        }

        public virtual void OnNavigatedFrom()
        { }

        public virtual Task OnNavigatedToAsync()
        {
            OnNavigatedTo();

            return Task.CompletedTask;
        }

        public virtual void OnNavigatedTo()
        { }

        private readonly ISnackbarService snackbarService;
        public ViewModel(ISnackbarService snackbarService)
        {
            this.snackbarService = snackbarService;
        }

        protected void RunWithErrorNotify(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                snackbarService.Show(
                    "エラー",
                    ex.Message,
                    Wpf.Ui.Controls.ControlAppearance.Caution,
                    icon: null,
                    timeout: TimeSpan.FromSeconds(5));
            }
        }
    }
}