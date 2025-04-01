using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Inventories
{
    public sealed class Location
    {
        public int? Id { get; }
        public string Name { get; }
        public string Description { get; }

        public static Location CreateNew(string name, string description)
        {
            return new Location(
                id: null,
                name: name,
                description: description);
        }

        public static Location FromPersistence(int id, string name, string description)
        {
            return new Location(
                id: id,
                name: name,
                description: description);
        }

        public Location(int? id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}