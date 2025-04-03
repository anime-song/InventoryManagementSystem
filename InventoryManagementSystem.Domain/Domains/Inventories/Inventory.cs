using System.Runtime.InteropServices;

namespace InventoryManagementSystem.Domain.Domains.Inventories
{
    public sealed record InventoryStatus
    {
        public static readonly InventoryStatus Active = new InventoryStatus(0);
        public static readonly InventoryStatus Split = new InventoryStatus(1);

        public int Value { get; }
        public InventoryStatus(int value)
        {
            Value = value;
        }
    }

    public sealed class Inventory
    {
        public int? Id { get; }
        public string ItemName { get; }
        public int LocationId { get; }
        public int Quantity { get; private set; }
        public DateTime RegisteredDate { get; }

        public InventoryStatus Status { get; private set; }
        public int? ParentInventoryId { get; private set; }
        public bool IsRoot => ParentInventoryId is null;

        public bool CanOperation() => Status == InventoryStatus.Active;

        public bool CanSplit() => Status == InventoryStatus.Active && IsRoot;

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
                id: null,
                itemName: itemName,
                quantity: initialQuantity,
                locationId: locationId,
                registeredDate: DateTime.Now,
                status: InventoryStatus.Active,
                parentInventoryId: null);
        }

        /// <summary>
        /// 分割用の在庫を作成します
        /// </summary>
        /// <param name="parentInventoryId"></param>
        /// <param name="itemName"></param>
        /// <param name="initialQuantity"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Inventory CreateFromSplit(
            int parentInventoryId,
            string itemName,
            int initialQuantity,
            int locationId)
        {
            if (initialQuantity < 0)
            {
                throw new ArgumentException("初期在庫は0以上である必要があります");
            }

            return new Inventory(
                id: null,
                itemName: itemName,
                quantity: initialQuantity,
                locationId: locationId,
                registeredDate: DateTime.Now,
                status: InventoryStatus.Active,
                parentInventoryId: parentInventoryId);
        }

        public static Inventory FromPersistence(
            int id,
            string itemName,
            int quantity,
            int locationId,
            DateTime registeredDate,
            InventoryStatus status,
            int? parentInventoryId)
        {
            return new Inventory(
                id: id,
                itemName: itemName,
                quantity: quantity,
                locationId: locationId,
                registeredDate: registeredDate,
                status: status,
                parentInventoryId: parentInventoryId);
        }

        public Inventory(
            int? id,
            string itemName,
            int quantity,
            int locationId,
            DateTime registeredDate,
            InventoryStatus status,
            int? parentInventoryId)
        {
            Id = id;
            ItemName = itemName;
            Quantity = quantity;
            LocationId = locationId;
            RegisteredDate = registeredDate;
            Status = status;
            ParentInventoryId = parentInventoryId;
        }

        /// <summary>
        /// 入庫処理を行います
        /// </summary>
        /// <param name="transferQuantity"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Store(int transferQuantity)
        {
            if (!CanOperation())
            {
                throw new InvalidOperationException("入庫ができない状態の在庫です");
            }
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
            if (!CanOperation())
            {
                throw new InvalidOperationException("出庫ができない状態の在庫です");
            }
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
            if (!CanOperation())
            {
                throw new InvalidOperationException("キャンセルができない状態の在庫です");
            }
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

        /// <summary>
        /// 分割済みの在庫とします
        /// </summary>
        public int MarkAsSplit()
        {
            if (!CanSplit())
            {
                throw new InvalidOperationException("分割ができない状態の在庫です");
            }
            if (Quantity <= 0)
            {
                throw new InvalidOperationException("分割のためには数量が1以上必要です");
            }

            var quantity = Quantity;
            Quantity = 0;
            Status = InventoryStatus.Split;
            return quantity;
        }
    }
}