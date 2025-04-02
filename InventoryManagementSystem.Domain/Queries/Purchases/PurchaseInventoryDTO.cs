using InventoryManagementSystem.Domain.Domains.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Queries.Purchases
{
    public sealed class PurchaseInventoryDTO
    {
        public PurchaseInventoryDTO(
            int inventoryId,
            string itemName,
            int purchaseId,
            int purchaseQuatnity,
            PurchaseStatus purchaseStatus,
            DateTime purchaseDate,
            int locationId,
            string locationName)
        {
            InventoryId = inventoryId;
            ItemName = itemName;
            PurchaseId = purchaseId;
            PurchaseQuatnity = purchaseQuatnity;
            PurchaseStatus = purchaseStatus;
            PurchaseDate = purchaseDate;
            LocationId = locationId;
            LocationName = locationName;
        }

        public int InventoryId { get; }
        public string ItemName { get; }
        public int PurchaseId { get; }
        public int PurchaseQuatnity { get; }
        public PurchaseStatus PurchaseStatus { get; }
        public DateTime PurchaseDate { get; }
        public int LocationId { get; }
        public string LocationName { get; }
    }
}