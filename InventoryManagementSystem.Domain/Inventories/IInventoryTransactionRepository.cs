using InventoryManagementSystem.Domain.Helpers;
using InventoryManagementSystem.Domain.Inventories.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Inventories
{
    public interface IInventoryTransactionRepository
    {
        IEnumerable<InventoryTransaction> FindAll();

        PageResult<InventoryTransaction> FindWithPaginate(FindTransactionRequest request, int page, int fetchCount);

        InventoryTransaction? FindById(int id);

        IEnumerable<InventoryTransaction> Find(FindTransactionRequest request);

        void Add(InventoryTransaction transaction);
    }
}