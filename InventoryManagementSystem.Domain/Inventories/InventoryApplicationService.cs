using InventoryManagementSystem.Domain.Helpers;
using InventoryManagementSystem.Domain.Inventories.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Inventories
{
    public sealed class InventoryApplicationService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly IInventoryTransactionRepository inventoryTransactionRepository;
        private readonly ILocationRepository locationRepository;

        public InventoryApplicationService(
            IInventoryRepository inventoryRepository,
            IInventoryTransactionRepository inventoryTransactionRepository,
            ILocationRepository locationRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.inventoryTransactionRepository = inventoryTransactionRepository;
            this.locationRepository = locationRepository;
        }

        /// <summary>
        /// 商品名で在庫を検索します
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public IEnumerable<Inventory> Find(string keyword)
        {
            return inventoryRepository.FindByItemName(keyword);
        }

        /// <summary>
        /// 在庫トランザクションを取得します
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IEnumerable<InventoryTransaction> FindTransaction(FindTransactionRequest request)
        {
            return inventoryTransactionRepository.Find(request);
        }

        /// <summary>
        /// ページ番号と件数を指定してトランザクションを取得します
        /// </summary>
        /// <remarks>
        /// ページ番号は1から開始になります。
        /// </remarks>
        /// <param name="transactionRequest"></param>
        /// <param name="page"></param>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        public PageResult<InventoryTransaction> FindTransactionWithPagenate(
            FindTransactionRequest transactionRequest,
            int page,
            int fetchCount)
        {
            return inventoryTransactionRepository.FindWithPaginate(transactionRequest, page, fetchCount);
        }

        /// <summary>
        /// 登録日が新しい順にfetchCountまで取得します
        /// </summary>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        public IEnumerable<Inventory> GetLatest(int fetchCount)
        {
            return inventoryRepository.GetLatest(fetchCount);
        }

        /// <summary>
        /// すべての保管場所を取得します
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Location> FindAllLocation()
        {
            return locationRepository.FindAll();
        }

        /// <summary>
        /// 新規保管場所を登録します
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public Location RegisterLocation(string name, string description)
        {
            var location = Location.CreateNew(name, description);
            return locationRepository.Add(location);
        }

        /// <summary>
        /// 保管場所を更新します
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public Location UpdateLocation(
            int locationId, string name, string description)
        {
            var location = locationRepository.FindById(locationId)
                ?? throw new InvalidOperationException($"ID={locationId}の保管場所が存在しません");

            location.Update(name, description);

            locationRepository.Update(location);
            return location;
        }

        /// <summary>
        /// 新規在庫を登録します
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="quantity"></param>
        /// <param name="locationId"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public Inventory Register(
            string itemName,
            int quantity,
            int locationId)
        {
            var inventory = Inventory.CreateNew(
                itemName: itemName,
                initialQuantity: quantity,
                locationId: locationId);
            // 採番されたIDを取得
            return inventoryRepository.Add(inventory);
        }

        /// <summary>
        /// 指定したInventoryIdで入庫処理を行います
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="quantity"></param>
        /// <param name="storeDate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Store(
            int inventoryId,
            int quantity,
            DateTime storeDate)
        {
            // 入庫処理
            var inventory = inventoryRepository.FindById(inventoryId)
                ?? throw new InvalidOperationException("指定された在庫が存在しません");
            inventory.Store(quantity);
            inventoryRepository.Update(inventory);

            // 在庫トランザクションの登録
            var transaction = InventoryTransaction.CreateNew(
                transactionType: TransactionType.In,
                transactionDate: storeDate,
                quantity: quantity,
                inventoryId: inventory.Id!.Value);
            inventoryTransactionRepository.Add(transaction);
        }

        /// <summary>
        /// 指定したInventoryIdで出庫処理を行います
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="quantity"></param>
        /// <param name="withdrawDate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Withdraw(
            int inventoryId,
            int quantity,
            DateTime withdrawDate)
        {
            // 出庫処理
            var inventory = inventoryRepository.FindById(inventoryId)
                ?? throw new InvalidOperationException("指定された在庫が存在しません");
            inventory.Withdraw(quantity);
            inventoryRepository.Update(inventory);

            // 在庫トランザクションの登録
            var transaction = InventoryTransaction.CreateNew(
                transactionType: TransactionType.Out,
                transactionDate: withdrawDate,
                quantity: quantity,
                inventoryId: inventory.Id!.Value);
            inventoryTransactionRepository.Add(transaction);
        }

        /// <summary>
        /// 指定したInventoryTransactionIdのトランザクションをキャンセルし、在庫をもとに戻します
        /// </summary>
        /// <param name="inventoryTransactionId"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CancelTransaction(
            int inventoryTransactionId)
        {
            var transaction = inventoryTransactionRepository.FindById(inventoryTransactionId)
                ?? throw new InvalidOperationException("指定された在庫トランザクションが存在しません");

            // 在庫をもとに戻す
            var inventory = inventoryRepository.FindById(transaction.InventoryId)
                ?? throw new InvalidOperationException("該当する在庫が存在しません");
            inventory.CancelTransaction(transaction);
            inventoryRepository.Update(inventory);

            // キャンセルトランザクションの登録
            var cancelTransaction = InventoryTransaction.CreateCancelTransaction(
                originalTransaction: transaction,
                cancelDate: DateTime.Now.Date);
            inventoryTransactionRepository.Add(cancelTransaction);
        }
    }
}