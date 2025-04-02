using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Domains.Purchases
{
    public sealed record PurchaseStatus
    {
        public static readonly PurchaseStatus Normal = new PurchaseStatus(0);
        public static readonly PurchaseStatus Cancelled = new PurchaseStatus(1);

        public string DisplayName
        {
            get
            {
                if (this == Normal)
                {
                    return "通常";
                }
                else if (this == Cancelled)
                {
                    return "取消済み";
                }

                return "未定義";
            }
        }

        public int Value { get; }
        public PurchaseStatus(int value)
        {
            Value = value;
        }
    }

    public sealed class Purchase
    {
        public int? Id { get; }
        public PurchaseStatus Status { get; private set; }
        public int InventoryId { get; }
        public int Quantity { get; }
        public DateTime PurchaseDate { get; }

        public static Purchase CreateNew(
            int inventoryId,
            DateTime purchaseDate,
            int quantity)
        {
            return new Purchase(
                id: null,
                status: PurchaseStatus.Normal,
                inventoryId: inventoryId,
                purchaseDate: purchaseDate,
                quantity: quantity);
        }

        public static Purchase FromPersistence(
            int id,
            PurchaseStatus status,
            int inventoryId,
            DateTime purchaseDate,
            int quantity)
        {
            return new Purchase(
                id: id,
                status: status,
                inventoryId: inventoryId,
                purchaseDate: purchaseDate,
                quantity: quantity);
        }

        public Purchase(
            int? id,
            PurchaseStatus status,
            int inventoryId,
            DateTime purchaseDate,
            int quantity)
        {
            Id = id;
            Status = status;
            InventoryId = inventoryId;
            PurchaseDate = purchaseDate;
            Quantity = quantity;
        }

        public void Cancel()
        {
            if (Status == PurchaseStatus.Cancelled)
            {
                throw new InvalidOperationException("すでにキャンセル済みの仕入です");
            }

            Status = PurchaseStatus.Cancelled;
        }
    }
}