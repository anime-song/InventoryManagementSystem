using InventoryManagementSystem.Domain.Domains.Purchases;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Purchases
{
    public sealed class PurchaseRepository : IPurchaseRepository
    {
        private readonly ILiteCollection<PurchaseEntity> _collection;

        public PurchaseRepository(LiteDatabase db)
        {
            _collection = db.GetCollection<PurchaseEntity>("purchase");
        }

        public IEnumerable<Purchase> FindAll()
        {
            return _collection.FindAll().Select(ToDomain);
        }

        public Purchase? FindById(int purchasedId)
        {
            var entity = _collection.FindById(purchasedId);

            return entity is null ? null : ToDomain(entity);
        }

        public Purchase Add(Purchase purchase)
        {
            var entity = ToEntity(purchase);
            var id = _collection.Insert(entity);

            entity.Id = id;
            return ToDomain(entity);
        }

        public void Update(Purchase purchase)
        {
            var entity = ToEntity(purchase);
            _collection.Update(entity);
        }

        public static Purchase ToDomain(PurchaseEntity entity)
        {
            return Purchase.FromPersistence(
                id: entity.Id,
                status: new PurchaseStatus(entity.Status),
                inventoryId: entity.InventoryId,
                purchaseDate: entity.PurchaseDate);
        }

        public static PurchaseEntity ToEntity(Purchase purchase)
        {
            return new PurchaseEntity()
            {
                Id = purchase.Id ?? 0,
                Status = purchase.Status.Value,
                InventoryId = purchase.InventoryId,
                PurchaseDate = purchase.PurchaseDate,
            };
        }
    }
}