using InventoryManagementSystem.Domain.Domains.Inventories;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class LocationRepository : ILocationRepository
    {
        private readonly ILiteCollection<LocationEntity> _collection;

        public LocationRepository(LiteDatabase db)
        {
            _collection = db.GetCollection<LocationEntity>("Location");
        }

        public Location Add(Location location)
        {
            var entity = ToEntity(location);
            entity.Id = 0;
            _collection.Insert(entity);

            return ToDomain(entity);
        }

        public IEnumerable<Location> FindAll()
        {
            return _collection.FindAll().ToList().Select(ToDomain);
        }

        public Location? FindById(int id)
        {
            var entity = _collection.FindById(id);

            return entity is null ? null : ToDomain(entity);
        }

        public Location Update(Location location)
        {
            _collection.Update(ToEntity(location));

            return location;
        }

        private static Location ToDomain(LocationEntity entity)
        {
            return Location.FromPersistence(
                id: entity.Id,
                name: entity.Name,
                description: entity.Description);
        }

        private static LocationEntity ToEntity(Location location)
        {
            return new LocationEntity()
            {
                Id = location.Id ?? 0,
                Name = location.Name,
                Description = location.Description,
            };
        }
    }
}