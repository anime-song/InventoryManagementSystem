using InventoryManagementSystem.Domain.Inventories;
using InventoryManagementSystem.Domain.Inventories.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Purchases
{
    public sealed class PurchaseCancellationService
    {
        private IPurchaseRepository purchaseRepository;
        private IInventoryApplicationService inventoryApplicationService;

        public PurchaseCancellationService(
            IPurchaseRepository purchaseRepository,
            IInventoryApplicationService inventoryApplicationService)
        {
            this.purchaseRepository = purchaseRepository;
            this.inventoryApplicationService = inventoryApplicationService;
        }

        /// <summary>
        /// 仕入がキャンセル可能かを取得します
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool CanCancelPurchase(Purchase purchase, IEnumerable<InventoryTransaction> transactions)
        {
            bool existsPurchaseTransaction = transactions.Any(
                x => x.TransactionSourceType == TransactionSourceType.Purchase &&
                x.SourceId == purchase.Id!.Value);
            // TODO: 売上や出庫、手動訂正などを考慮する
            bool hasNoInventoryMovement = transactions.Count() == 1;

            return existsPurchaseTransaction && hasNoInventoryMovement;
        }
    }
}