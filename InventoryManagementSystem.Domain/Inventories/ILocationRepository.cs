using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Inventories
{
    public interface ILocationRepository
    {
        IEnumerable<Location> FindAll();

        Location? FindById(int id);

        Location Add(Location location);

        Location Update(Location location);
    }
}