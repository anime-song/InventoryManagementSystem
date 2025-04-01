using InventoryManagementSystem.Domain.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.WPF.Inventories
{
    public sealed class InventoryListDisplayModel
    {
        public int Id { get; init; }
        public string ItemName { get; init; }
        public int Quantity { get; init; }
        public string LocationName { get; init; }
        public DateTime RegisteredDate { get; init; }

        public static InventoryListDisplayModel FromInventory(
            Inventory inventory,
            IEnumerable<Location> locations)
        {
            return new InventoryListDisplayModel()
            {
                Id = inventory.Id!.Value,
                ItemName = inventory.ItemName,
                Quantity = inventory.Quantity,
                LocationName = locations.FirstOrDefault(x => x.Id == inventory.LocationId)?.Name ?? string.Empty,
                RegisteredDate = inventory.RegisteredDate
            };
        }
    }
}