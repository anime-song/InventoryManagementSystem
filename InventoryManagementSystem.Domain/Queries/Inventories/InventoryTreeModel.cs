using InventoryManagementSystem.Domain.Domains.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Queries.Inventories
{
    public sealed class InventoryTreeModel
    {
        public InventoryTreeModel(
            int id,
            string itemName,
            int locationId,
            int quantity,
            DateTime registeredDate,
            InventoryStatus status,
            int? parentInventoryId,
            IReadOnlyList<InventoryTreeModel> children)
        {
            Id = id;
            ItemName = itemName;
            LocationId = locationId;
            Quantity = quantity;
            RegisteredDate = registeredDate;
            Status = status;
            ParentInventoryId = parentInventoryId;
            Children = children;
        }

        public int Id { get; }
        public string ItemName { get; }
        public int LocationId { get; }
        public int Quantity { get; }
        public DateTime RegisteredDate { get; }

        public InventoryStatus Status { get; }
        public int? ParentInventoryId { get; }

        public IReadOnlyList<InventoryTreeModel> Children { get; }
    }
}