using System.Runtime.InteropServices;

namespace InventoryManagementSystem.Domain.Domains.Inventories
{
    public sealed class Inventory
    {
        public int? Id { get; }
        public string ItemName { get; }
        public int LocationId { get; }
        public int Quantity { get; private set; }
        public DateTime RegisteredDate { get; }

        /// <summary>
        /// 新規在庫を作成します
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="initialQuantity"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Inventory CreateNew(string itemName, int initialQuantity, int locationId)
        {
            if (initialQuantity < 0)
            {
                throw new ArgumentException("初期在庫は0以上である必要があります");
            }

            return new Inventory(
                id: null, itemName: itemName, quantity: initialQuantity, locationId: locationId, registeredDate: DateTime.Now);
        }

        public static Inventory FromPersistence(int id, string itemName, int quantity, int locationId, DateTime registeredDate)
        {
            return new Inventory(
                id: id, itemName: itemName, quantity: quantity, locationId: locationId, registeredDate: registeredDate);
        }

        public Inventory(int? id, string itemName, int quantity, int locationId, DateTime registeredDate)
        {
            Id = id;
            ItemName = itemName;
            Quantity = quantity;
            LocationId = locationId;
            RegisteredDate = registeredDate;
        }

        /// <summary>
        /// 入庫処理を行います
        /// </summary>
        /// <param name="transferQuantity"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Store(int transferQuantity)
        {
            if (transferQuantity < 1)
            {
                throw new ArgumentException("入庫数量は1以上である必要があります");
            }

            Quantity += transferQuantity;
        }

        /// <summary>
        /// 出庫処理を行います
        /// </summary>
        /// <param name="transferQuantity"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Withdraw(int transferQuantity)
        {
            if (transferQuantity < 1)
            {
                throw new ArgumentException("出庫数量は1以上である必要があります");
            }

            Quantity -= transferQuantity;

            if (Quantity < 0)
            {
                throw new InvalidOperationException("在庫数がマイナスになるため出庫ができません");
            }
        }

        /// <summary>
        /// 在庫トランザクションから在庫の数量をもとに戻します
        /// </summary>
        /// <param name="transaction"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CancelTransaction(InventoryTransaction transaction)
        {
            if (transaction.TransactionType != TransactionType.In &&
                transaction.TransactionType != TransactionType.Out)
            {
                throw new InvalidOperationException("このトランザクションはキャンセルできません");
            }

            // 入庫処理なら減らし
            if (transaction.TransactionType == TransactionType.In)
            {
                Quantity -= transaction.Quantity;
            }
            // 出庫処理なら増やす
            else if (transaction.TransactionType == TransactionType.Out)
            {
                Quantity += transaction.Quantity;
            }

            // TODO: 在庫数がマイナスになるケースも想定しないと不便な可能性
            if (Quantity < 0)
            {
                throw new InvalidOperationException("在庫数がマイナスになるためキャンセルできません");
            }
        }
    }
}