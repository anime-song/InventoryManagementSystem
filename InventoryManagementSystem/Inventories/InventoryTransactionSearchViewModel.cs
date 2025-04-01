using InventoryManagementSystem.Domain.Inventories;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed record InventoryTransactionSearchCondition
    {
        public int? InventoryId { get; init; }
        public DateTime? TransactionDateFrom { get; init; }
        public DateTime? TransactionDateTo { get; init; }
        public TransactionType? SelectedTransactionType { get; init; }
    }

    public sealed class InventoryTransactionSearchViewModel
    {
        public ReactiveProperty<int?> InventoryId { get; } = new ReactiveProperty<int?>();
        public ReactiveProperty<DateTime?> TransactionDateFrom { get; } = new ReactiveProperty<DateTime?>();
        public ReactiveProperty<DateTime?> TransactionDateTo { get; } = new ReactiveProperty<DateTime?>();
        public ReactiveProperty<TransactionType?> SelectedTransactionType { get; } = new ReactiveProperty<TransactionType?>();

        private InventoryTransactionSearchCondition? confirmSearchCondition = null;

        public InventoryTransactionSearchCondition? GetConfirmedCondition()
        {
            return confirmSearchCondition;
        }

        public void Confirm()
        {
            confirmSearchCondition = ToCondition();
        }

        private InventoryTransactionSearchCondition ToCondition() => new InventoryTransactionSearchCondition()
        {
            InventoryId = InventoryId.Value,
            TransactionDateFrom = TransactionDateFrom.Value,
            TransactionDateTo = TransactionDateTo.Value,
            SelectedTransactionType = SelectedTransactionType.Value,
        };

        public void Clear()
        {
            InventoryId.Value = null;
            TransactionDateFrom.Value = null;
            TransactionDateTo.Value = null;
            SelectedTransactionType.Value = null;
            confirmSearchCondition = null;
        }
    }
}