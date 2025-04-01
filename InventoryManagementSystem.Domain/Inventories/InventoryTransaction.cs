namespace InventoryManagementSystem.Domain.Inventories
{
    public sealed record TransactionType
    {
        public static readonly TransactionType In = new TransactionType(0);
        public static readonly TransactionType Out = new TransactionType(1);
        public static readonly TransactionType Cancel = new TransactionType(2);

        public static readonly IReadOnlyList<TransactionType> Items = new List<TransactionType>()
        {
            In, Out, Cancel
        };

        public string DisplayName
        {
            get
            {
                if (this == In)
                {
                    return "入庫";
                }
                else if (this == Out)
                {
                    return "出庫";
                }

                return "キャンセル";
            }
        }

        public int Value { get; }

        public TransactionType(int value)
        {
            Value = value;
        }
    }

    public sealed class InventoryTransaction
    {
        public int? Id { get; }
        public TransactionType TransactionType { get; }
        public DateTime TransactionDate { get; }
        public int Quantity { get; }
        public int InventoryId { get; }

        public int? CanceledTransactionId { get; }

        public static InventoryTransaction CreateNew(
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId)
        {
            return new InventoryTransaction(
                id: null,
                transactionType: transactionType,
                transactionDate: transactionDate,
                quantity: quantity,
                inventoryId: inventoryId,
                canceledTransactionId: null);
        }

        /// <summary>
        /// キャンセルトランザクションを作成します
        /// </summary>
        /// <param name="originalTransaction"></param>
        /// <param name="cancelDate"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static InventoryTransaction CreateCancelTransaction(
            InventoryTransaction originalTransaction,
            DateTime cancelDate)
        {
            if (originalTransaction.TransactionType == TransactionType.Cancel)
            {
                throw new InvalidOperationException("Cancelトランザクションはキャンセルできません");
            }
            if (originalTransaction.Id is null)
            {
                throw new InvalidOperationException("永続化されていないトランザクションはキャンセルできません");
            }

            return new InventoryTransaction(
                id: null,
                transactionType: TransactionType.Cancel,
                transactionDate: cancelDate,
                quantity: -originalTransaction.Quantity,
                inventoryId: originalTransaction.InventoryId,
                canceledTransactionId: originalTransaction.Id);
        }

        public static InventoryTransaction FromPersistence(
            int id,
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId,
            int? canceledTransactionId)
        {
            return new InventoryTransaction(
                id: id,
                transactionType: transactionType,
                transactionDate: transactionDate,
                quantity: quantity,
                inventoryId: inventoryId,
                canceledTransactionId: canceledTransactionId);
        }

        public InventoryTransaction(
            int? id,
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId,
            int? canceledTransactionId)
        {
            Id = id;
            TransactionType = transactionType;
            TransactionDate = transactionDate;
            Quantity = quantity;
            InventoryId = inventoryId;
            CanceledTransactionId = canceledTransactionId;
        }
    }
}