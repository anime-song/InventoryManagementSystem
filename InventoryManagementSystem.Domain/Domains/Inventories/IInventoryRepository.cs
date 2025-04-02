using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Domains.Inventories
{
    public interface IInventoryRepository
    {
        IEnumerable<Inventory> FindAll();

        Inventory? FindById(int id);

        IEnumerable<Inventory> FindByItemName(string keyword);

        IEnumerable<Inventory> GetLatest(int fetchCount);

        Inventory Add(Inventory inventory);

        Inventory Update(Inventory inventory);
    }
}