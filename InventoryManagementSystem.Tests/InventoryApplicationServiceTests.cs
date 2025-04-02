using InventoryManagementSystem.Domain.Inventories;
using Moq;

namespace InventoryManagementSystem.Tests
{
    public sealed class InventoryApplicationServiceTests
    {
        [Fact(DisplayName = "�������݌ɓo�^�����")]
        public void Register_ShouldRegisterInventory()
        {
            var mockInventoryRepo = new Mock<IInventoryRepository>();
            var mockTransactionRepo = new Mock<IInventoryTransactionRepository>();
            var mockLocationRepo = new Mock<ILocationRepository>();
            var service = new InventoryApplicationService(mockInventoryRepo.Object, mockTransactionRepo.Object, mockLocationRepo.Object);

            service.Register(itemName: "�{", quantity: 4, locationId: 2);

            mockInventoryRepo.Verify(r => r.Add(It.Is<Inventory>(
                i => i.ItemName == "�{" && i.Quantity == 4 && i.LocationId == 2)), Times.Once());
        }

        [Fact(DisplayName = "�݌ɂ����݂���ꍇ���������ɂ����ʂ����Z�����")]
        public void Store_ShouldAddInventoryTransaction_WhenInventoryExists()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Fact(DisplayName = "�݌ɂ����݂���ꍇ�������o�ɂ����ʂ����Z�����")]
        public void Withdraw_ShouldReduceInventoryAndAddTransaction_WhenSufficientInventory()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Theory(DisplayName = "�݌ɂ����݂��o�ɐ��ʂ��݌ɐ��ʂƓ���������ȉ��̏ꍇ�������o�ɂł���")]
        [InlineData(4, 1)]
        [InlineData(5, 0)]
        public void Withdraw_ShouldSucceed_WhenQuantityIsLessThanOrEqualToStock(int withdrawQuantity, int expectedRemaining)
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Theory(DisplayName = "�������g�����U�N�V�����L�����Z���������s����")]
        [MemberData(nameof(CancelTransactionTestCases))]
        public void CancelTransaction_ShouldCancelTransaction(TransactionType transactionType, int expectedRemaining)
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Fact(DisplayName = "�݌ɂ��Ȃ��ꍇ��O�𓊂���")]
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

        [Fact(DisplayName = "�݌Ƀg�����U�N�V�������Ȃ��ꍇ��O�𓊂���")]
        public void CancelTransaction_ShouldThrow_WhenInventoryTransactionDoesNotExists()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Fact(DisplayName = "�g�����U�N�V�����L�����Z�����ɍ݌ɂ��}�C�i�X�ɂȂ�ꍇ��O�𓊂���")]
        public void CancelTransaction_ShouldThrow_WhenInventoryQuantityWouldBecomeNegative()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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

        [Fact(DisplayName = "�݌ɂ����݂��Ȃ��ꍇ��O�𓊂���")]
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

        [Fact(DisplayName = "�݌ɂ����݂��o�ɐ��ʂ��݌ɐ��ʂ�葽���ꍇ��O�𓊂���")]
        public void Withdraw_ShouldThrow_WhenQuantityExceedsInventoryQuantity()
        {
            var inventory = Inventory.FromPersistence(id: 1, itemName: "�{", quantity: 5, locationId: 2, registeredDate: DateTime.Now);
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