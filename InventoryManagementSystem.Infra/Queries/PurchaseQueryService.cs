using InventoryManagementSystem.Domain.Domains.Purchases;
using InventoryManagementSystem.Domain.Queries.Purchases;
using InventoryManagementSystem.Infra.Inventories;
using InventoryManagementSystem.Infra.Purchases;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Queries
{
    public sealed class PurchaseQueryService : IPurchaseQueryService
    {
        private readonly ILiteCollection<PurchaseEntity> purchaseCollection;
        private readonly ILiteCollection<InventoryEntity> inventoryCollection;
        private readonly ILiteCollection<LocationEntity> locationCollection;

        public PurchaseQueryService(LiteDatabase db)
        {
            purchaseCollection = db.GetCollection<PurchaseEntity>("purchase");
            inventoryCollection = db.GetCollection<InventoryEntity>("inventory");
            locationCollection = db.GetCollection<LocationEntity>("location");
        }

        public IEnumerable<PurchaseInventoryDTO> GetLatest(int fetchCount)
        {
            var purchases = purchaseCollection
                .Query()
                .OrderByDescending(x => x.Id)
                .Limit(fetchCount)
                .ToList();
            var inventoryIds = purchases.Select(x => x.InventoryId).ToHashSet();

            var inventories = inventoryCollection
                .Query()
                .Where(x => inventoryIds.Contains(x.Id))
                .ToList();
            var locationIds = inventories.Select(x => x.LocationId).ToHashSet();

            var locations = locationCollection
                .Query()
                .Where(x => locationIds.Contains(x.Id))
                .ToList();

            return purchases.Select(x => ToPurchaseInventoryDTO(x, inventories, locations));
        }

        private static PurchaseInventoryDTO ToPurchaseInventoryDTO(
            PurchaseEntity entity,
            IEnumerable<InventoryEntity> inventories,
            IEnumerable<LocationEntity> locations)
        {
            var inventory = inventories.First(x => x.Id == entity.InventoryId);
            var location = locations.First(x => x.Id == inventory.LocationId);

            return new PurchaseInventoryDTO(
                inventoryId: inventory.Id,
                itemName: inventory.ItemName,
                purchaseId: entity.Id,
                purchaseQuatnity: entity.Quantity,
                purchaseStatus: new PurchaseStatus(entity.Status),
                purchaseDate: entity.PurchaseDate,
                locationId: inventory.LocationId,
                locationName: location.Name);
        }
    }
}