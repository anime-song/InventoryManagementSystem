using InventoryManagementSystem.Domain.Domains.Inventories;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class InventoryRepository : IInventoryRepository
    {
        private readonly ILiteCollection<InventoryEntity> _collection;

        public InventoryRepository(LiteDatabase db)
        {
            _collection = db.GetCollection<InventoryEntity>("Inventory");
        }

        public IEnumerable<Inventory> FindAll()
        {
            return _collection.FindAll().Select(ToDomain);
        }

        public Inventory? FindById(int id)
        {
            var entity = _collection.FindById(id);
            return entity is null ? null : ToDomain(entity);
        }

        public IEnumerable<Inventory> FindByItemName(string keyword)
        {
            return _collection
                .Find(x => x.ItemName.Contains(keyword))
                .Select(ToDomain);
        }

        public IEnumerable<Inventory> GetLatest(int fetchCount)
        {
            return _collection
                .Query()
                .OrderByDescending(x => x.RegisteredDate)
                .Limit(fetchCount)
                .ToList()
                .Select(ToDomain);
        }

        public Inventory Add(Inventory inventory)
        {
            var entity = ToEntity(inventory);
            var newId = _collection.Insert(entity);

            // Insert後に採番されたIDをentityに設定して、ドメインモデルに変換
            entity.Id = newId.AsInt32;
            return ToDomain(entity);
        }

        public Inventory Update(Inventory inventory)
        {
            _collection.Update(ToEntity(inventory));
            return inventory;
        }

        private static Inventory ToDomain(InventoryEntity entity)
        {
            return Inventory.FromPersistence(
                id: entity.Id,
                itemName: entity.ItemName,
                quantity: entity.Quantity,
                locationId: entity.LocationId,
                registeredDate: entity.RegisteredDate,
                status: new InventoryStatus(entity.Status),
                parentInventoryId: entity.ParentInventoryId);
        }

        private static InventoryEntity ToEntity(Inventory inventory)
        {
            return new InventoryEntity
            {
                Id = inventory.Id ?? 0, // 0にするとIDの自動採番
                ItemName = inventory.ItemName,
                LocationId = inventory.LocationId,
                Quantity = inventory.Quantity,
                RegisteredDate = inventory.RegisteredDate,
                Status = inventory.Status.Value,
                ParentInventoryId = inventory.ParentInventoryId
            };
        }
    }
}