using System.Security.AccessControl;

namespace InventoryManagementSystem.Domain.Domains.Inventories
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

    public sealed record TransactionSourceType
    {
        public static readonly TransactionSourceType Purchase = new TransactionSourceType(0);
        public static readonly TransactionSourceType Sales = new TransactionSourceType(1);
        public static readonly TransactionSourceType Manual = new TransactionSourceType(2);
        public static readonly TransactionSourceType Split = new TransactionSourceType(3);
        public static readonly TransactionSourceType SplitSource = new TransactionSourceType(4);

        public int Value { get; }
        public TransactionSourceType(int value)
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

        public TransactionSourceType TransactionSourceType { get; }
        public int? SourceId { get; }

        public static InventoryTransaction CreateNew(
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId,
            TransactionSourceType sourceType,
            int? sourceId)
        {
            return new InventoryTransaction(
                id: null,
                transactionType: transactionType,
                transactionDate: transactionDate,
                quantity: quantity,
                inventoryId: inventoryId,
                canceledTransactionId: null,
                sourceType: sourceType,
                sourceId: sourceId);
        }

        public static InventoryTransaction CreateFromSplitSource(
            int quantity,
            int inventoryId)
        {
            return new InventoryTransaction(
                id: null,
                transactionType: TransactionType.Out,
                transactionDate: DateTime.Now,
                quantity: quantity,
                inventoryId: inventoryId,
                canceledTransactionId: null,
                sourceType: TransactionSourceType.SplitSource,
                sourceId: null);
        }

        public static InventoryTransaction FromPersistence(
            int id,
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId,
            int? canceledTransactionId,
            TransactionSourceType sourceType,
            int? sourceId)
        {
            return new InventoryTransaction(
                id: id,
                transactionType: transactionType,
                transactionDate: transactionDate,
                quantity: quantity,
                inventoryId: inventoryId,
                canceledTransactionId: canceledTransactionId,
                sourceType: sourceType,
                sourceId: sourceId);
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
            DateTime cancelDate,
            TransactionSourceType sourceType,
            int? sourceId)
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
                canceledTransactionId: originalTransaction.Id,
                sourceType: sourceType,
                sourceId: sourceId);
        }

        public InventoryTransaction(
            int? id,
            TransactionType transactionType,
            DateTime transactionDate,
            int quantity,
            int inventoryId,
            int? canceledTransactionId,
            TransactionSourceType sourceType,
            int? sourceId)
        {
            Id = id;
            TransactionType = transactionType;
            TransactionDate = transactionDate;
            Quantity = quantity;
            InventoryId = inventoryId;
            CanceledTransactionId = canceledTransactionId;
            TransactionSourceType = sourceType;
            SourceId = sourceId;
        }
    }
}