using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class LocationEntity
    {
        [BsonId]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}