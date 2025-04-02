using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Inventories.Requests;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.Domain.Domains.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Applications.Purchases
{
    public interface IPurchaseApplicationService
    {
        IEnumerable<Purchase> FindAll();

        Purchase RegisterPurchase(
            string itemName,
            DateTime purchaseDate,
            int quantity,
            int locationId);

        void CancelPurchase(int purchaseId);
    }

    public sealed class PurchaseApplicationService : IPurchaseApplicationService
    {
        private readonly IPurchaseRepository purchaseRepository;
        private readonly IInventoryApplicationService inventoryApplicationService;
        private readonly PurchaseCancellationService purchaseCancellationService;

        public PurchaseApplicationService(
            IPurchaseRepository purchaseRepository,
            IInventoryApplicationService inventoryApplicationService)
        {
            this.purchaseRepository = purchaseRepository;
            this.inventoryApplicationService = inventoryApplicationService;
            this.purchaseCancellationService = new PurchaseCancellationService(purchaseRepository, inventoryApplicationService);
        }

        public IEnumerable<Purchase> FindAll()
        {
            return purchaseRepository.FindAll();
        }

        /// <summary>
        /// 仕入処理を行います
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="purchaseDate"></param>
        /// <param name="quantity"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public Purchase RegisterPurchase(
            string itemName,
            DateTime purchaseDate,
            int quantity,
            int locationId)
        {
            // 新規在庫作成
            var registeredInventory = inventoryApplicationService.Register(
                itemName: itemName,
                quantity: 0,
                locationId: locationId);

            // 仕入
            var purchase = Purchase.CreateNew(
                inventoryId: registeredInventory.Id!.Value,
                purchaseDate: purchaseDate,
                quantity: quantity);
            purchase = purchaseRepository.Add(purchase);

            // 入庫
            inventoryApplicationService.Store(
                inventoryId: registeredInventory.Id!.Value,
                quantity: quantity,
                storeDate: purchaseDate,
                sourceType: TransactionSourceType.Purchase,
                sourceId: purchase.Id!.Value);

            return purchase;
        }

        /// <summary>
        /// 仕入キャンセル処理を行います
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CancelPurchase(
            int purchaseId)
        {
            var purchase = purchaseRepository.FindById(purchaseId)
                ?? throw new InvalidOperationException($"ID: {purchaseId}の仕入が存在しません");

            var transactions = inventoryApplicationService.FindTransaction(new FindTransactionRequest()
            {
                InventoryId = purchase.InventoryId,
            });

            if (!purchaseCancellationService.CanCancelPurchase(purchase, transactions))
            {
                throw new InvalidOperationException("指定した仕入はキャンセルできません");
            }

            var purchaseTransaction = transactions.FirstOrDefault(
                x => x.TransactionSourceType == TransactionSourceType.Purchase && x.SourceId == purchase.Id)
                ?? throw new InvalidOperationException("仕入に対応する入庫トランザクションが存在しません");

            purchase.Cancel();
            purchaseRepository.Update(purchase);

            inventoryApplicationService.CancelTransaction(
                inventoryTransactionId: purchaseTransaction.Id!.Value,
                sourceType: TransactionSourceType.Purchase,
                sourceId: purchase.Id!.Value);
        }
    }
}