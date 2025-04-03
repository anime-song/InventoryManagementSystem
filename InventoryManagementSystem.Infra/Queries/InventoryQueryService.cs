using InventoryManagementSystem.Domain.Domains.Inventories;
using InventoryManagementSystem.Domain.Queries.Inventories;
using InventoryManagementSystem.Infra.Inventories;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Queries
{
    public sealed class InventoryQueryService : IInventoryQueryService
    {
        private readonly ILiteCollection<InventoryEntity> collection;

        public InventoryQueryService(LiteDatabase db)
        {
            collection = db.GetCollection<InventoryEntity>("inventory");
        }

        public IEnumerable<InventoryTreeModel> GetLatest(int fetchCount)
        {
            var rootInventories = collection.Query()
                .OrderByDescending(x => x.Id)
                .Where(x => x.ParentInventoryId == null)
                .Limit(fetchCount)
                .ToList();

            var result = new List<InventoryTreeModel>();
            foreach (var root in rootInventories)
            {
                var children = collection
                    .Query()
                    .Where(x => x.ParentInventoryId == root.Id)
                    .ToList()
                    .Select(x => new InventoryTreeModel(
                        id: x.Id,
                        itemName: x.ItemName,
                        locationId: x.LocationId,
                        quantity: x.Quantity,
                        registeredDate: x.RegisteredDate,
                        status: new InventoryStatus(x.Status),
                        parentInventoryId: x.ParentInventoryId,
                        children: []));

                result.Add(new InventoryTreeModel(
                    id: root.Id,
                    itemName: root.ItemName,
                    locationId: root.LocationId,
                    quantity: root.Quantity,
                    registeredDate: root.RegisteredDate,
                    status: new InventoryStatus(root.Status),
                    parentInventoryId: root.ParentInventoryId,
                    children: children.ToList()));
            }

            return result;
        }
    }
}