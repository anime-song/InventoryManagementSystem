using InventoryManagementSystem.Domain.Applications;
using InventoryManagementSystem.Domain.Applications.Inventories;
using InventoryManagementSystem.Domain.Applications.Inventories.Requests;
using InventoryManagementSystem.Domain.Applications.Purchases;
using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.Domain.Domains.Purchases;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests
{
    public sealed class PurchaseApplicationServiceTests
    {
        [Fact(DisplayName = "正しく仕入処理が行われる")]
        public void RegisterPurchase_CreatesInventoryAndPurchaseAndStoreTransaction()
        {
            var mockPurchase = new Mock<IPurchaseRepository>();
            var mockInventoryService = new Mock<IInventoryApplicationService>();
            var cancelService = new PurchaseCancellationService(mockPurchase.Object, mockInventoryService.Object);

            var inventoryId = 100;
            var inventory = Inventory.FromPersistence(
                id: inventoryId, itemName: "商品A", quantity: 10, locationId: 1, registeredDate: DateTime.Now.Date);
            mockInventoryService.Setup(r => r.Register(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(inventory);

            var purchaseId = 200;
            mockPurchase.Setup(r => r.Add(It.IsAny<Purchase>()))
                .Returns<Purchase>(x =>
                {
                    return Purchase.FromPersistence(
                        id: purchaseId,
                        status: x.Status,
                        inventoryId: x.InventoryId,
                        purchaseDate: x.PurchaseDate);
                });

            var service = new PurchaseApplicationService(
                mockPurchase.Object,
                mockInventoryService.Object,
                cancelService);

            service.RegisterPurchase(
                itemName: "商品A",
                purchaseDate: DateTime.Today,
                quantity: 10,
                locationId: 1);

            mockInventoryService.Verify(r => r.Register("商品A", 0, 1), Times.Once);
            mockInventoryService.Verify(r => r.Store(inventoryId, 10, DateTime.Today, TransactionSourceType.Purchase, 200), Times.Once);
            mockPurchase.Verify(r => r.Add(It.Is<Purchase>(
                x => x.InventoryId == inventoryId &&
                x.Status == PurchaseStatus.Normal &&
                x.PurchaseDate == DateTime.Today
                )),
                Times.Once);
        }

        [Fact(DisplayName = "仕入キャンセル処理が正しく行われる")]
        public void CancelPurchase_ShouldUpdateStatusAndCallCancelTransaction()
        {
            var mockPurchase = new Mock<IPurchaseRepository>();
            var mockInventoryService = new Mock<IInventoryApplicationService>();
            var cancelService = new PurchaseCancellationService(mockPurchase.Object, mockInventoryService.Object);

            var inventoryId = 100;
            var purchaseId = 200;

            var transaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: TransactionType.In,
                transactionDate: DateTime.Now.Date,
                quantity: 10,
                inventoryId: inventoryId,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Purchase,
                sourceId: purchaseId);
            mockInventoryService.Setup(r => r.FindTransaction(It.Is<FindTransactionRequest>(x => x.InventoryId == inventoryId)))
                .Returns(new List<InventoryTransaction>() { transaction });

            var purchase = Purchase.FromPersistence(
                        id: purchaseId,
                        status: PurchaseStatus.Normal,
                        inventoryId: inventoryId,
                        purchaseDate: DateTime.Now.Date);
            mockPurchase.Setup(r => r.FindById(It.Is<int>(x => x == purchaseId)))
                .Returns(purchase);

            var service = new PurchaseApplicationService(
                mockPurchase.Object,
                mockInventoryService.Object,
                cancelService);

            service.CancelPurchase(purchaseId);

            mockPurchase.Verify(r => r.Update(It.Is<Purchase>(x => x.Status == PurchaseStatus.Cancelled)), Times.Once);
            mockInventoryService.Verify(r => r.CancelTransaction(1, TransactionSourceType.Purchase, purchaseId), Times.Once);
        }

        [Fact(DisplayName = "仕入キャンセル処理で仕入が存在しない場合例外を投げる")]
        public void CancelPurchase_ShouldThrows_WhenDoesNotExistsPurchase()
        {
            var mockPurchase = new Mock<IPurchaseRepository>();
            var mockInventoryService = new Mock<IInventoryApplicationService>();
            var cancelService = new PurchaseCancellationService(mockPurchase.Object, mockInventoryService.Object);

            var inventoryId = 100;
            var purchaseId = 200;

            var transaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: TransactionType.In,
                transactionDate: DateTime.Now.Date,
                quantity: 10,
                inventoryId: inventoryId,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Purchase,
                sourceId: purchaseId);
            mockInventoryService.Setup(r => r.FindTransaction(It.Is<FindTransactionRequest>(x => x.InventoryId == inventoryId)))
                .Returns(new List<InventoryTransaction>() { transaction });

            var service = new PurchaseApplicationService(
                mockPurchase.Object,
                mockInventoryService.Object,
                cancelService);

            Assert.Throws<InvalidOperationException>(() =>
            {
                service.CancelPurchase(purchaseId);
            });
        }

        [Fact(DisplayName = "仕入キャンセル処理でキャンセルが不可の状態の場合は例外を投げる")]
        public void CancelPurchase_ShouldThrow_WhenCanCancelIsFalse()
        {
            var mockPurchase = new Mock<IPurchaseRepository>();
            var mockInventoryService = new Mock<IInventoryApplicationService>();
            var cancelService = new PurchaseCancellationService(mockPurchase.Object, mockInventoryService.Object);

            var inventoryId = 100;
            var purchaseId = 200;

            var transaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: TransactionType.In,
                transactionDate: DateTime.Now.Date,
                quantity: 10,
                inventoryId: inventoryId,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Manual,
                sourceId: purchaseId);
            mockInventoryService.Setup(r => r.FindTransaction(It.Is<FindTransactionRequest>(x => x.InventoryId == inventoryId)))
                .Returns(new List<InventoryTransaction>() { transaction });

            var purchase = Purchase.FromPersistence(
                        id: purchaseId,
                        status: PurchaseStatus.Normal,
                        inventoryId: inventoryId,
                        purchaseDate: DateTime.Now.Date);
            mockPurchase.Setup(r => r.FindById(It.Is<int>(x => x == purchaseId)))
                .Returns(purchase);

            var service = new PurchaseApplicationService(
                mockPurchase.Object,
                mockInventoryService.Object,
                cancelService);

            Assert.Throws<InvalidOperationException>(() =>
            {
                service.CancelPurchase(purchaseId);
            });

            mockPurchase.Verify(x => x.Update(It.IsAny<Purchase>()), Times.Never);
            mockInventoryService.Verify(x => x.CancelTransaction(
                It.IsAny<int>(), It.IsAny<TransactionSourceType>(), It.IsAny<int>()), Times.Never);
        }
    }
}