using InventoryManagementSystem.Domain.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed class RecentInventoryDisplayModel
    {
        public int InventoryId { get; init; }
        public string ItemName { get; init; }
        public int Quantity { get; init; }
        public DateTime RegisteredDate { get; init; }
        public string LocationName { get; init; }

        public static RecentInventoryDisplayModel FromInventory(
            Inventory inventory,
            IEnumerable<Location> locations)
        {
            return new RecentInventoryDisplayModel
            {
                InventoryId = inventory.Id!.Value,
                ItemName = inventory.ItemName,
                Quantity = inventory.Quantity,
                RegisteredDate = inventory.RegisteredDate,
                LocationName = locations.FirstOrDefault(x => x.Id == inventory.LocationId)?.Name ?? string.Empty
            };
        }
    }
}