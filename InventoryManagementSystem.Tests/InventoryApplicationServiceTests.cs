using InventoryManagementSystem.Domain.Inventories;
using Moq;

namespace InventoryManagementSystem.Tests
{
    public sealed class InventoryApplicationServiceTests
    {
        [Fact(DisplayName = "正しく在庫登録される")]
        public void Register_ShouldRegisterInventory()
        {
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.Register(itemName: "本", quantity: 4, locationId: 2);

            mockInventoryRepo.Verify(r => r.Add(It.Is<Inventory>(
                i => i.ItemName == "本" && i.Quantity == 4 && i.LocationId == 2)), Times.Once());
        }

        [Fact(DisplayName = "在庫が存在する場合正しく入庫し数量が加算される")]
        public void Store_ShouldAddInventoryTransaction_WhenInventoryExists()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.Store(
                inventoryId: 1, quantity: 4, storeDate: DateTime.Today, sourceType: TransactionSourceType.Manual, sourceId: null);

            mockInventoryRepo.Verify(r => r.Update(It.Is<Inventory>(i => i.Quantity == 9)), Times.Once);
            mockTransactionRepo.Verify(r => r.Add(It.IsAny<InventoryTransaction>()), Times.Once);
        }

        [Fact(DisplayName = "在庫が存在する場合正しく出庫し数量が減算される")]
        public void Withdraw_ShouldReduceInventoryAndAddTransaction_WhenSufficientInventory()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.Withdraw(
                inventoryId: 1, quantity: 3, withdrawDate: DateTime.Today, sourceType: TransactionSourceType.Manual, sourceId: null);

            mockInventoryRepo.Verify(r => r.Update(It.Is<Inventory>(i => i.Quantity == 2)), Times.Once);
            mockTransactionRepo.Verify(r => r.Add(It.IsAny<InventoryTransaction>()), Times.Once);
        }

        [Theory(DisplayName = "在庫が存在し出庫数量が在庫数量と同じかそれ以下の場合正しく出庫できる")]
        [InlineData(4, 1)]
        [InlineData(5, 0)]
        public void Withdraw_ShouldSucceed_WhenQuantityIsLessThanOrEqualToStock(int withdrawQuantity, int expectedRemaining)
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.Withdraw(
                inventoryId: 1, quantity: withdrawQuantity, withdrawDate: DateTime.Today, sourceType: TransactionSourceType.Manual, sourceId: null);

            mockInventoryRepo.Verify(r => r.Update(It.Is<Inventory>(i => i.Quantity == expectedRemaining)), Times.Once);
            mockTransactionRepo.Verify(r => r.Add(It.IsAny<InventoryTransaction>()), Times.Once);
        }

        public static IEnumerable<object[]> CancelTransactionTestCases => new List<object[]>()
        {
            new object[] { TransactionType.In, 2},
            new object[] { TransactionType.Out, 8 }
        };

        [Theory(DisplayName = "正しくトランザクションキャンセル処理が行える")]
        [MemberData(nameof(CancelTransactionTestCases))]
        public void CancelTransaction_ShouldCancelTransaction(TransactionType transactionType, int expectedRemaining)
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var inventoryTransaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: transactionType,
                transactionDate: DateTime.Today,
                quantity: 3,
                inventoryId: 1,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Manual,
                sourceId: null);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            mockTransactionRepo.Setup(r => r.FindById(1)).Returns(inventoryTransaction);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.CancelTransaction(inventoryTransactionId: 1, sourceType: TransactionSourceType.Manual, sourceId: null);

            mockInventoryRepo.Verify(r => r.Update(It.Is<Inventory>(i => i.Quantity == expectedRemaining)), Times.Once);
            mockTransactionRepo.Verify(r => r.Add(It.Is<InventoryTransaction>(
                i => i.CanceledTransactionId == inventoryTransaction.Id && i.TransactionType == TransactionType.Cancel)),
                Times.Once);
        }

        [Fact(DisplayName = "在庫がない場合例外を投げる")]
        public void CancelTransaction_ShouldThrow_WhenInventoryDoesNotExists()
        {
            var inventoryTransaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: TransactionType.In,
                transactionDate: DateTime.Today,
                quantity: 3,
                inventoryId: 1,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Manual,
                sourceId: null);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns((Inventory?)null);
            mockTransactionRepo.Setup(r => r.FindById(1)).Returns(inventoryTransaction);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                service.CancelTransaction(inventoryTransactionId: 1, sourceType: TransactionSourceType.Manual, sourceId: null);
            });
        }

        [Fact(DisplayName = "在庫トランザクションがない場合例外を投げる")]
        public void CancelTransaction_ShouldThrow_WhenInventoryTransactionDoesNotExists()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            mockTransactionRepo.Setup(r => r.FindById(1)).Returns((InventoryTransaction?)null);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                service.CancelTransaction(inventoryTransactionId: 1, sourceType: TransactionSourceType.Manual, sourceId: null);
            });
        }

        [Fact(DisplayName = "トランザクションキャンセル時に在庫がマイナスになる場合例外を投げる")]
        public void CancelTransaction_ShouldThrow_WhenInventoryQuantityWouldBecomeNegative()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var inventoryTransaction = InventoryTransaction.FromPersistence(
                id: 1,
                transactionType: TransactionType.In,
                transactionDate: DateTime.Today,
                quantity: 6,
                inventoryId: 1,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.Manual,
                sourceId: null);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            mockTransactionRepo.Setup(r => r.FindById(1)).Returns(inventoryTransaction);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                service.CancelTransaction(inventoryTransactionId: 1, sourceType: TransactionSourceType.Manual, sourceId: null);
            });
        }

        [Fact(DisplayName = "在庫が存在しない場合例外を投げる")]
        public void Withdraw_ShouldThrow_WhenInventoryDoesNotExists()
        {
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(It.IsAny<int>())).Returns((Inventory?)null);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            Assert.Throws<InvalidOperationException>(
                () => service.Withdraw(
                    inventoryId: 1, quantity: 3, withdrawDate: DateTime.Today, sourceType: TransactionSourceType.Manual, sourceId: null));
        }

        [Fact(DisplayName = "在庫が存在し出庫数量が在庫数量より多い場合例外を投げる")]
        public void Withdraw_ShouldThrow_WhenQuantityExceedsInventoryQuantity()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "本", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();

            mockInventoryRepo.Setup(r => r.FindById(1)).Returns(inventory);
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            Assert.Throws<InvalidOperationException>(
                () => service.Withdraw(
                    inventoryId: 1, quantity: 6, withdrawDate: DateTime.Today, sourceType: TransactionSourceType.Manual, sourceId: null));
        }
    }
}