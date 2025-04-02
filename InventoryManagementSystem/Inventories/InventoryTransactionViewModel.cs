using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Inventories.Requests;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.WPF.Controls;
using InventoryManagementSystem.WPF.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed class InventoryTransactionViewModel : ViewModel
    {
        public IReadOnlyList<TransactionType> TransactionTypes => TransactionType.Items;

        public InventoryTransactionSearchViewModel InventoryTransactionSearchViewModel { get; } = new InventoryTransactionSearchViewModel();
        public PaginationControlViewModel PaginationControlViewModel { get; } = new PaginationControlViewModel();

        private readonly IInventoryApplicationService inventoryApplicationService;

        public InventoryTransactionViewModel(
            IInventoryApplicationService inventoryApplicationService,
            ISnackbarService snackbarService) : base(snackbarService)
        {
            this.inventoryApplicationService = inventoryApplicationService;

            LoadInventoryTransaction(PaginationControlViewModel.CurrentPage.Value);
            PageChangedCommand = new ReactiveCommand<int>().WithSubscribe(LoadInventoryTransaction);
            PaginationControlViewModel.PageChangedCommand = PageChangedCommand;

            SearchCommand = new ReactiveCommand().WithSubscribe(OnSearch);
        }

        public ReactiveCollection<InventoryTransaction> InventoryTransactions { get; } = new ReactiveCollection<InventoryTransaction>();

        public ReactiveCommand<int> PageChangedCommand { get; }

        private void LoadInventoryTransaction(int page)
        {
            var request = new FindTransactionRequest();
            // 確定した検索パラメーターがあれば使う
            var searchCondition = InventoryTransactionSearchViewModel.GetConfirmedCondition();
            if (searchCondition is not null)
            {
                request.InventoryId = searchCondition.InventoryId;
                request.TransactionPeriodStart = searchCondition.TransactionDateFrom;
                request.TransactionPeriodEnd = searchCondition.TransactionDateTo;
                // TODO: Type
            }

            InventoryTransactions.Clear();
            var pageResult = inventoryApplicationService.FindTransactionWithPagenate(
                request,
                page: page,
                fetchCount: 30);

            foreach (var transaction in pageResult.PageData)
            {
                InventoryTransactions.Add(transaction);
            }

            PaginationControlViewModel.LastPage.Value = pageResult.TotalPages;
        }

        public ReactiveCommand SearchCommand { get; }

        private void OnSearch()
        {
            InventoryTransactionSearchViewModel.Confirm();
            LoadInventoryTransaction(1);
        }
    }
}